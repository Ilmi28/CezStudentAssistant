using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Common;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Services;

public class AIService(
    IAIClient aiClient,
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IJobService jobService,
    IConfiguration configuration) : IAIService, IScopedService
{
    private readonly string _containerName = configuration["BlobContainerSettings:CourseFilesContainer"]
        ?? configuration["CourseFilesContainer"]
        ?? "course-files";

    public async Task GenerateQuiz(GenerateQuizDto dto, CancellationToken ct = default)
    {
        var job = await jobService.GetLatestJobAsync(dto.UserId, JobType.QuizGeneration, ct);
        if (job == null)
            throw new AppException(AIMessageConsts.JobNotFound);

        await jobService.UpdateJobAsync(job, JobStatus.Processing, ct: ct);

        var aiFiles = new List<AIFile>();
        try
        {
            var courseRepo = unitOfWork.Repository<ICourseRepository>();
            var course = await courseRepo.GetByIdAsync(dto.CourseId, ct);
            if (course == null)
                throw new NotFoundException(AIMessageConsts.CourseNotFound);

            var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
            List<Resource> resources = await resourceRepo.Find(r => r.CourseId == dto.CourseId).ToListAsync(ct);

            foreach (var resource in resources)
            {
                var filePath = $"{dto.CourseId}/{resource.Name}";
                var stream = await fileService.DownloadAsync(filePath, _containerName, ct);
                aiFiles.Add(new AIFile
                {
                    Stream = stream,
                    MimeType = resource.MimeType
                });
            }

            var aiRequest = new AIQuizRequest
            {
                QuestionCount = dto.QuestionCount,
                Language = dto.Language,
                Files = aiFiles,
                AdditionalInstructions = dto.AdditionalInstructions
            };

            var aiResponse = await aiClient.GenerateQuizAsync(aiRequest);

            if (!aiResponse.Success || aiResponse.Data == null || !aiResponse.Data.Questions.Any())
            {
                throw new BadRequestException(aiResponse.Message ?? AIMessageConsts.QuizGenerationError);
            }

            var quiz = new Quiz
            {
                Name = aiResponse.Data.Title,
                DisplayName = aiResponse.Data.Title,
                CourseId = dto.CourseId,
                Questions = aiResponse.Data.Questions.Select(q => new Question
                {
                    Content = q.Content,
                    Type = (Domain.Enums.QuestionType)q.QuestionType,
                    Points = q.Points,
                    Options = q.Options.Select(o => new QuestionOption
                    {
                        Content = o.Content,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            };

            var quizRepo = unitOfWork.Repository<IQuizRepository>();
            await quizRepo.AddAsync(quiz, ct);
            await unitOfWork.SaveChangesAsync(ct);

            await jobService.UpdateJobAsync(job, JobStatus.Succeeded, ct: ct);
        }
        catch
        {
            await jobService.UpdateJobAsync(job, JobStatus.Failed, ct: ct);
            throw;
        }
        finally
        {
            foreach (var file in aiFiles)
            {
                try
                {
                    file.Stream?.Dispose();
                }
                catch
                {
                }
            }
        }
    }
}
