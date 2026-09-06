using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.Flashcard;

public sealed class EstimateFlashcardTokensQuery : IQuery<EstimateFlashcardTokensDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public int CardCount { get; set; } = 10;
    public string? AdditionalInstructions { get; set; }
    public bool GenerateFromPromptOnly { get; set; }
}

public class EstimateFlashcardTokensQueryHandler(
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IAIClient aiClient,
    IConfiguration configuration) : BaseQueryHandler<EstimateFlashcardTokensQuery, EstimateFlashcardTokensDto>
{
    private const int EstimatedTokensPerOutputCard = 150;

    private readonly string _containerName = configuration["BlobContainerSettings:CourseFilesContainer"]
        ?? throw new InvalidOperationException(CourseMessageConsts.CourseFilesContainerConfigMissing);

    protected override string SuccessMessage => FlashcardMessageConsts.EstimateFlashcardTokensSuccess;
    protected override string ErrorMessage => FlashcardMessageConsts.EstimateFlashcardTokensError;

    protected override async Task<EstimateFlashcardTokensDto> ExecuteAsync(EstimateFlashcardTokensQuery query, CancellationToken ct)
    {
        var courseRepo = unitOfWork.Repository<ICourseRepository>();
        var course = await courseRepo.GetByIdAsync(query.CourseId, ct)
            ?? throw new NotFoundException(AIMessageConsts.CourseNotFound);

        var maxTokensConfig = configuration["Gemini:MaximumDailyTokens"];
        if (string.IsNullOrWhiteSpace(maxTokensConfig) || !int.TryParse(maxTokensConfig, out var dailyTokenLimit) || dailyTokenLimit <= 0)
        {
            throw new InvalidOperationException(UserMessageConsts.MaximumDailyTokensConfigMissing);
        }

        var tokenUsageRepo = unitOfWork.Repository<ITokenUsageRepository>();
        var (completedTokens, reservedTokens) = await tokenUsageRepo.GetDailyTokenUsageBreakdownAsync(query.UserId, DateTime.UtcNow, ct);
        if (completedTokens == 0 && reservedTokens == 0)
        {
            var fallbackUsed = await tokenUsageRepo.GetDailyTokenUsageAsync(query.UserId, DateTime.UtcNow, ct);
            if (fallbackUsed > 0)
            {
                completedTokens = fallbackUsed;
            }
        }
        var totalUsedAndReserved = completedTokens + reservedTokens;

        var inputTokens = await CalculateInputTokensAsync(query, ct);

        var estimatedOutputTokens = Math.Max(1, query.CardCount) * EstimatedTokensPerOutputCard;
        var totalEstimatedTokens = inputTokens + estimatedOutputTokens;

        var estimatedPercentage = Math.Round((double)totalEstimatedTokens / dailyTokenLimit * 100, 2);
        var remainingDailyTokens = Math.Max(0, dailyTokenLimit - totalUsedAndReserved);
        var canGenerate = (totalUsedAndReserved + totalEstimatedTokens) <= dailyTokenLimit;

        return new EstimateFlashcardTokensDto
        {
            EstimatedTokens = totalEstimatedTokens,
            DailyTokenLimit = dailyTokenLimit,
            DailyTokensUsed = completedTokens,
            DailyTokensReserved = reservedTokens,
            EstimatedDailyUsagePercentage = estimatedPercentage,
            RemainingDailyTokens = remainingDailyTokens,
            CanGenerate = canGenerate
        };
    }

    private async Task<int> CalculateInputTokensAsync(EstimateFlashcardTokensQuery query, CancellationToken ct)
    {
        if (query.GenerateFromPromptOnly)
        {
            return await aiClient.EstimateTokenUsageAsync(new AIQuizRequest
            {
                QuestionCount = query.CardCount,
                AdditionalInstructions = query.AdditionalInstructions,
                GenerateFromPromptOnly = true,
                Files = []
            });
        }

        var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
        var resources = await resourceRepo.Find(r => r.CourseId == query.CourseId && !r.IsHidden).ToListAsync(ct);
        var inputTokens = 0;

        foreach (var resource in resources)
        {
            if (resource.EstimatedTokens <= 0)
            {
                await using var stream = await fileService.DownloadAsync($"{query.CourseId}/{resource.Name}", _containerName, ct);

                var aiFile = new AIFile
                {
                    Stream = stream,
                    MimeType = resource.MimeType
                };

                var tokens = await aiClient.EstimateTokenUsageAsync(new AIQuizRequest
                {
                    QuestionCount = 1,
                    Files = new[] { aiFile }
                });

                if (tokens > 0)
                {
                    resource.EstimatedTokens = tokens;
                    await resourceRepo.UpdateAsync(resource, ct);
                    await unitOfWork.SaveChangesAsync(ct);
                }
            }

            inputTokens += resource.EstimatedTokens;
        }

        if (!string.IsNullOrWhiteSpace(query.AdditionalInstructions))
        {
            var promptTokens = await aiClient.EstimateTextTokenUsageAsync(query.AdditionalInstructions);
            inputTokens += promptTokens;
        }

        return inputTokens;
    }
}
