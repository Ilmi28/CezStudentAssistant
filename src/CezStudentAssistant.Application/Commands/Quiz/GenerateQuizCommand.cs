using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CezStudentAssistant.Application.Commands.Quiz;

public sealed class GenerateQuizCommand : ICommand, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public int QuestionCount { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public string? AdditionalInstructions { get; set; }
    public int? EasyQuestionCountPerAttempt { get; set; }
    public int? MediumQuestionCountPerAttempt { get; set; }
    public int? HardQuestionCountPerAttempt { get; set; }
    public int? QuestionCountPerAttempt { get; set; }
}

public class GenerateQuizCommandHandler(
    IJobScheduler jobScheduler,
    IJobService jobService,
    IUnitOfWork unitOfWork,
    IFileService? fileService = null,
    IAIClient? aiClient = null,
    IConfiguration? configuration = null) : BaseCommandHandler<GenerateQuizCommand>
{
    private const int EstimatedTokensPerOutputQuestion = 200;

    protected override string SuccessMessage => AIMessageConsts.QuizGenerationEnqueued;
    protected override string ErrorMessage => AIMessageConsts.QuizGenerationError;

    protected override async Task ExecuteAsync(GenerateQuizCommand command, CancellationToken ct)
    {
        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepo.GetByIdAsync(command.CourseId, ct);
        if (course == null)
            throw new NotFoundException(AIMessageConsts.CourseNotFound);

        var quizRepo = unitOfWork.Repository<IQuizRepository>();
        var existingQuizzes = quizRepo.Find(q => q.CourseId == command.CourseId).ToList();
        var quizTitle = $"Quiz #{existingQuizzes.Count + 1}";

        var reservedCount = await EstimateTokensAsync(command.CourseId, command.QuestionCount, ct);

        var tokenUsageRepo = unitOfWork.Repository<ITokenUsageRepository>();
        var reservation = new Domain.Entities.TokenUsage
        {
            UserId = command.UserId,
            UsageType = Domain.Enums.UsageTokenType.ReservedQuizGeneration,
            UsageCount = reservedCount
        };
        await tokenUsageRepo.AddAsync(reservation, ct);

        var quiz = new Domain.Entities.Quiz
        {
            UserId = command.UserId,
            Name = quizTitle,
            CourseId = command.CourseId,
            Status = QuizStatusEnum.Generating,
            TimeLimitMinutes = command.TimeLimitMinutes,
            EasyQuestionCountPerAttempt = command.EasyQuestionCountPerAttempt,
            MediumQuestionCountPerAttempt = command.MediumQuestionCountPerAttempt,
            HardQuestionCountPerAttempt = command.HardQuestionCountPerAttempt,
            QuestionCountPerAttempt = command.QuestionCountPerAttempt
        };

        await quizRepo.AddAsync(quiz, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var job = await jobService.CreateJobAsync(command.UserId, JobType.QuizGeneration, ct);

        var dto = new GenerateQuizDto
        {
            QuizId = quiz.Id,
            UserId = command.UserId,
            CourseId = command.CourseId,
            ReservationId = reservation.Id,
            QuestionCount = command.QuestionCount,
            TimeLimitMinutes = command.TimeLimitMinutes,
            Language = QuizLanguage.PL,
            AdditionalInstructions = command.AdditionalInstructions,
            EasyQuestionCountPerAttempt = command.EasyQuestionCountPerAttempt,
            MediumQuestionCountPerAttempt = command.MediumQuestionCountPerAttempt,
            HardQuestionCountPerAttempt = command.HardQuestionCountPerAttempt
        };

        var jobId = jobScheduler.Enqueue<IQuizGenerationService>(service => service.GenerateQuiz(dto, ct));
        await jobService.UpdateJobAsync(job, JobStatus.Enqueued, jobId, ct);
    }

    private async Task<int> EstimateTokensAsync(Guid courseId, int questionCount, CancellationToken ct)
    {
        var outputTokens = Math.Max(1, questionCount) * EstimatedTokensPerOutputQuestion;
        var inputTokens = 0;

        var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
        var resources = resourceRepo.Find(r => r.CourseId == courseId).ToList();

        var updatedAny = false;
        var containerName = configuration?["BlobContainerSettings:CourseFilesContainer"];

        foreach (var resource in resources)
        {
            if (resource.EstimatedTokens <= 0 && fileService != null && aiClient != null && !string.IsNullOrWhiteSpace(containerName))
            {
                try
                {
                    await using var stream = await fileService.DownloadAsync($"{courseId}/{resource.Name}", containerName, ct);
                    if (stream != null)
                    {
                        var tokens = await aiClient.EstimateTokenUsageAsync(new AIQuizRequest
                        {
                            QuestionCount = 0,
                            Files = [new AIFile { Stream = stream, MimeType = resource.MimeType }]
                        });

                        if (tokens > 0)
                        {
                            resource.EstimatedTokens = tokens;
                            updatedAny = true;
                        }
                    }
                }
                catch
                {
                    // Ignore estimation failures during reservation fallback
                }
            }

            inputTokens += resource.EstimatedTokens;
        }

        if (updatedAny)
        {
            try
            {
                await unitOfWork.SaveChangesAsync(ct);
            }
            catch
            {
                // Ignore save errors for updated resource estimation
            }
        }

        var total = inputTokens + outputTokens;
        return Math.Max(2000, total);
    }
}
