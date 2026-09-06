using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Helpers;
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
    private readonly string _containerName = configuration["BlobContainerSettings:CourseFilesContainer"]
        ?? throw new InvalidOperationException(CourseMessageConsts.CourseFilesContainerConfigMissing);

    public async Task GenerateQuiz(GenerateQuizDto dto, CancellationToken ct = default)
    {
        var job = await jobService.GetLatestJobAsync(dto.UserId, JobType.QuizGeneration, ct)
            ?? throw new AppException(AIMessageConsts.JobNotFound);

        await jobService.UpdateJobAsync(job, JobStatus.Processing, ct: ct);

        if (dto.QuizId != Guid.Empty)
        {
            var quizRepo = unitOfWork.Repository<IQuizRepository>();
            var quiz = await quizRepo.GetByIdAsync(dto.QuizId, ct);
            if (quiz != null && quiz.Status != QuizStatusEnum.Generating)
            {
                quiz.Status = QuizStatusEnum.Generating;
                await quizRepo.UpdateAsync(quiz, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }
        }

        var aiFiles = new List<AIFile>();
        try
        {
            var maxTokensConfig = configuration["Gemini:MaximumDailyTokens"];
            if (string.IsNullOrWhiteSpace(maxTokensConfig) || !int.TryParse(maxTokensConfig, out var dailyTokenLimit) || dailyTokenLimit <= 0)
            {
                throw new InvalidOperationException(UserMessageConsts.MaximumDailyTokensConfigMissing);
            }

            var tokenUsageRepo = unitOfWork.Repository<ITokenUsageRepository>();
            var dailyTokensUsed = await tokenUsageRepo.GetDailyTokenUsageAsync(dto.UserId, DateTime.UtcNow, ct);

            aiFiles = dto.GenerateFromPromptOnly
                ? new List<AIFile>()
                : await DownloadCourseFilesAsync(dto.CourseId, ct);

            var aiRequest = new AIQuizRequest
            {
                QuestionCount = dto.QuestionCount,
                Language = dto.Language,
                Files = aiFiles,
                AdditionalInstructions = dto.AdditionalInstructions,
                EasyCount = dto.EasyQuestionCountPerAttempt,
                MediumCount = dto.MediumQuestionCountPerAttempt,
                HardCount = dto.HardQuestionCountPerAttempt,
                GenerateFromPromptOnly = dto.GenerateFromPromptOnly
            };

            var estimatedTokens = await aiClient.EstimateTokenUsageAsync(aiRequest);
            var estimatedOutputTokens = Math.Max(1, dto.QuestionCount) * 200;
            var totalEstimatedTokens = estimatedTokens + estimatedOutputTokens;

            if (dailyTokensUsed + totalEstimatedTokens > dailyTokenLimit)
            {
                throw new BadRequestException(AIMessageConsts.DailyTokenLimitExceeded);
            }

            var aiResponse = await aiClient.GenerateQuizAsync(aiRequest);

            if (!aiResponse.Success || aiResponse.Data == null || !aiResponse.Data.Questions.Any())
            {
                throw new BadRequestException(aiResponse.Message ?? AIMessageConsts.QuizGenerationError);
            }

            await CommitTokenUsageAsync(dto.UserId, dto.ReservationId, aiResponse.TotalTokens, ct);
            await SaveQuizWithQuestionsAsync(dto, aiResponse.Data, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await jobService.UpdateJobAsync(job, JobStatus.Succeeded, ct: ct);
        }
        catch
        {
            await ReleaseTokenReservationAsync(dto.ReservationId, ct);
            await MarkQuizAsFailedAsync(dto.QuizId, ct);
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
        var resources = await resourceRepo.Find(r => r.CourseId == courseId && !r.IsHidden).ToListAsync(ct);

        var files = new List<AIFile>();
        foreach (var resource in resources)
        {
            var stream = await fileService.DownloadAsync($"{courseId}/{resource.Name}", _containerName, ct);
            files.Add(new AIFile { Stream = stream, MimeType = resource.MimeType });
        }
        return files;
    }

    private async Task CommitTokenUsageAsync(Guid userId, Guid reservationId, int tokenCount, CancellationToken ct)
    {
        var tokenUsageRepo = unitOfWork.Repository<ITokenUsageRepository>();
        if (reservationId != Guid.Empty)
        {
            var reservation = await tokenUsageRepo.GetByIdAsync(reservationId, ct);
            if (reservation != null)
            {
                reservation.UsageType = UsageTokenType.QuizGeneration;
                reservation.UsageCount = tokenCount;
                return;
            }
        }

        await tokenUsageRepo.AddAsync(new TokenUsage
        {
            UserId = userId,
            UsageType = UsageTokenType.QuizGeneration,
            UsageCount = tokenCount
        }, ct);
    }

    private async Task ReleaseTokenReservationAsync(Guid reservationId, CancellationToken ct)
    {
        if (reservationId == Guid.Empty) return;
        var tokenUsageRepo = unitOfWork.Repository<ITokenUsageRepository>();
        var reservation = await tokenUsageRepo.GetByIdAsync(reservationId, ct);
        if (reservation != null)
        {
            await tokenUsageRepo.DeleteAsync(reservation, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    private async Task SaveQuizWithQuestionsAsync(GenerateQuizDto dto, AIQuiz aiQuiz, CancellationToken ct)
    {
        var quizRepo = unitOfWork.Repository<IQuizRepository>();
        var questionRepo = unitOfWork.Repository<IQuestionRepository>();
        Quiz? quiz = dto.QuizId != Guid.Empty ? await quizRepo.GetByIdAsync(dto.QuizId, ct, includes: q => q.Questions) : null;

        if (quiz == null)
        {
            var existingQuizzes = quizRepo.Find(q => q.CourseId == dto.CourseId).ToList();
            var quizNumberTitle = $"Quiz #{existingQuizzes.Count + 1}";
            var finalTitle = BuildQuizTitle(quizNumberTitle, aiQuiz.Title);

            quiz = new Quiz
            {
                UserId = dto.UserId,
                Name = finalTitle,
                CourseId = dto.CourseId,
                Status = QuizStatusEnum.Ready,
                TimeLimitMinutes = dto.TimeLimitMinutes,
                QuestionCountPerAttempt = Math.Min(5, aiQuiz.Questions.Count)
            };
            await quizRepo.AddAsync(quiz, ct);
            foreach (var question in MapQuestions(quiz, aiQuiz.Questions))
            {
                await questionRepo.AddAsync(question, ct);
            }
        }
        else
        {
            var quizNumberTitle = !string.IsNullOrWhiteSpace(quiz.Name) && quiz.Name.StartsWith("Quiz #", StringComparison.OrdinalIgnoreCase)
                ? quiz.Name
                : $"Quiz #{quizRepo.Find(q => q.CourseId == dto.CourseId).Count()}";

            var finalTitle = BuildQuizTitle(quizNumberTitle, aiQuiz.Title);

            quiz.Name = finalTitle;
            quiz.Status = QuizStatusEnum.Ready;
            foreach (var question in MapQuestions(quiz, aiQuiz.Questions))
            {
                await questionRepo.AddAsync(question, ct);
            }
        }
    }

    private static string BuildQuizTitle(string quizNumberTitle, string? aiTitle)
    {
        if (string.IsNullOrWhiteSpace(aiTitle))
            return quizNumberTitle;

        var topic = aiTitle.Trim();

        if (topic.EndsWith(" - Quiz", StringComparison.OrdinalIgnoreCase))
        {
            topic = topic[..^7].Trim();
        }

        if (topic.StartsWith("Quiz z ", StringComparison.OrdinalIgnoreCase))
        {
            topic = topic[7..].Trim();
        }

        if (string.IsNullOrWhiteSpace(topic) ||
            topic.Equals("Quiz", StringComparison.OrdinalIgnoreCase) ||
            topic.Equals(quizNumberTitle, StringComparison.OrdinalIgnoreCase) ||
            topic.StartsWith("Quiz #", StringComparison.OrdinalIgnoreCase))
        {
            return quizNumberTitle;
        }

        return $"{quizNumberTitle} - {topic}";
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
                Difficulty = q.Difficulty
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

    private async Task MarkQuizAsFailedAsync(Guid quizId, CancellationToken ct)
    {
        if (quizId == Guid.Empty) return;

        var quizRepo = unitOfWork.Repository<IQuizRepository>();
        var quiz = await quizRepo.GetByIdAsync(quizId, ct);
        if (quiz != null)
        {
            quiz.Status = QuizStatusEnum.Failed;
            await quizRepo.UpdateAsync(quiz, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    private static void DisposeFiles(IEnumerable<AIFile> files)
    {
        foreach (var file in files)
        {
            file.Stream?.Dispose();
        }
    }
}
