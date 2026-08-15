using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Common;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses.AI.Quiz;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CezStudentAssistant.Application.Services;

public class QuizGenerationService(
    IAIClient aiClient,
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IJobService jobService,
    IConfiguration configuration) : IQuizGenerationService, IScopedService
{
    private readonly string _containerName = configuration["BlobContainerSettings:CourseFilesContainer"] ?? string.Empty;

    public async Task GenerateQuiz(GenerateQuizDto dto, CancellationToken ct = default)
    {
        var job = await jobService.GetLatestJobAsync(dto.UserId, JobType.QuizGeneration, ct)
            ?? throw new AppException(AIMessageConsts.JobNotFound);

        await jobService.UpdateJobAsync(job, JobStatus.Processing, ct: ct);

        var aiFiles = new List<AIFile>();
        try
        {
            aiFiles = await DownloadCourseFilesAsync(dto.CourseId, ct);

            var aiResponse = await aiClient.GenerateQuizAsync(new AIQuizRequest
            {
                QuestionCount = dto.QuestionCount,
                Language = dto.Language,
                Files = aiFiles,
                AdditionalInstructions = dto.AdditionalInstructions
            });

            if (!aiResponse.Success || aiResponse.Data == null || !aiResponse.Data.Questions.Any())
            {
                throw new BadRequestException(aiResponse.Message ?? AIMessageConsts.QuizGenerationError);
            }

            await RecordTokenUsageAsync(dto.UserId, aiResponse.TotalTokens, ct);
            await SaveQuizWithQuestionsAsync(dto, aiResponse.Data, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await jobService.UpdateJobAsync(job, JobStatus.Succeeded, ct: ct);
        }
        catch
        {
            await CleanupGeneratingQuizAsync(dto.QuizId, ct);
            await jobService.UpdateJobAsync(job, JobStatus.Failed, ct: ct);
            throw;
        }
        finally
        {
            DisposeFiles(aiFiles);
        }
    }

    private async Task<List<AIFile>> DownloadCourseFilesAsync(Guid courseId, CancellationToken ct)
    {
        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepo.GetByIdAsync(courseId, ct)
            ?? throw new NotFoundException(AIMessageConsts.CourseNotFound);

        var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
        var resources = await resourceRepo.Find(r => r.CourseId == courseId).ToListAsync(ct);

        var files = new List<AIFile>();
        foreach (var resource in resources)
        {
            var stream = await fileService.DownloadAsync($"{courseId}/{resource.Name}", _containerName, ct);
            files.Add(new AIFile { Stream = stream, MimeType = resource.MimeType });
        }
        return files;
    }

    private async Task RecordTokenUsageAsync(Guid userId, int tokenCount, CancellationToken ct)
    {
        var tokenUsageRepo = unitOfWork.Repository<ITokenUsageRepository>();
        await tokenUsageRepo.AddAsync(new TokenUsage
        {
            UserId = userId,
            UsageType = UsageTokenType.QuizGeneration,
            UsageCount = tokenCount
        }, ct);
    }

    private async Task SaveQuizWithQuestionsAsync(GenerateQuizDto dto, AIQuiz aiQuiz, CancellationToken ct)
    {
        var quizRepo = unitOfWork.Repository<IQuizRepository>();
        var questionRepo = unitOfWork.Repository<IQuestionRepository>();
        Quiz? quiz = dto.QuizId != Guid.Empty ? await quizRepo.GetByIdAsync(dto.QuizId, ct, includes: q => q.Questions) : null;

        if (quiz == null)
        {
            var existingQuizzes = quizRepo.Find(q => q.CourseId == dto.CourseId).ToList();
            var defaultTitle = $"Quiz #{existingQuizzes.Count + 1}";
            var finalTitle = !string.IsNullOrWhiteSpace(aiQuiz.Title) && !aiQuiz.Title.StartsWith("Quiz z", StringComparison.OrdinalIgnoreCase)
                ? aiQuiz.Title
                : defaultTitle;

            quiz = new Quiz
            {
                UserId = dto.UserId,
                Name = finalTitle,
                DisplayName = finalTitle,
                CourseId = dto.CourseId,
                Status = QuizStatusEnum.Ready
            };
            await quizRepo.AddAsync(quiz, ct);
            foreach (var question in MapQuestions(quiz, aiQuiz.Questions))
            {
                await questionRepo.AddAsync(question, ct);
            }
        }
        else
        {
            var finalTitle = !string.IsNullOrWhiteSpace(aiQuiz.Title) && !aiQuiz.Title.StartsWith("Quiz z", StringComparison.OrdinalIgnoreCase)
                ? aiQuiz.Title
                : (!string.IsNullOrWhiteSpace(quiz.DisplayName) ? quiz.DisplayName : quiz.Name);

            quiz.Name = finalTitle;
            quiz.DisplayName = finalTitle;
            quiz.Status = QuizStatusEnum.Ready;
            foreach (var question in MapQuestions(quiz, aiQuiz.Questions))
            {
                await questionRepo.AddAsync(question, ct);
            }
        }
    }

    private static List<Question> MapQuestions(Quiz quiz, List<AIQuestion> aiQuestions)
    {
        return aiQuestions.Select(q =>
        {
            var question = new Question
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                Quiz = quiz,
                Content = q.Content,
                Type = (QuestionType)q.QuestionType,
                Points = q.Points
            };
            question.Options = q.Options.Select(o => new QuestionOption
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                Question = question,
                Content = o.Content,
                IsCorrect = o.IsCorrect
            }).ToList();
            return question;
        }).ToList();
    }

    private async Task CleanupGeneratingQuizAsync(Guid quizId, CancellationToken ct)
    {
        if (quizId == Guid.Empty) return;

        try
        {
            var quizRepo = unitOfWork.Repository<IQuizRepository>();
            var quiz = await quizRepo.GetByIdAsync(quizId, ct);
            if (quiz is { Status: QuizStatusEnum.Generating })
            {
                await quizRepo.DeleteAsync(quiz, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }
        }
        catch
        {
            // Ignore cleanup failures on error path
        }
    }

    private static void DisposeFiles(IEnumerable<AIFile> files)
    {
        foreach (var file in files)
        {
            try { file.Stream?.Dispose(); } catch { }
        }
    }
}
