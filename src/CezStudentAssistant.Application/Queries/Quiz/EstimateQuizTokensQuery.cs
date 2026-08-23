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

namespace CezStudentAssistant.Application.Queries.Quiz;

public sealed class EstimateQuizTokensQuery : IQuery<EstimateQuizTokensDto>, IUserRequest
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public int QuestionCount { get; set; } = 5;
    public string? AdditionalInstructions { get; set; }
}

public class EstimateQuizTokensQueryHandler(
    IUnitOfWork unitOfWork,
    IFileService fileService,
    IAIClient aiClient,
    IConfiguration configuration) : BaseQueryHandler<EstimateQuizTokensQuery, EstimateQuizTokensDto>
{
    private const int EstimatedTokensPerOutputQuestion = 200;

    private readonly string _containerName = configuration["BlobContainerSettings:CourseFilesContainer"]
        ?? throw new InvalidOperationException(CourseMessageConsts.CourseFilesContainerConfigMissing);

    protected override string SuccessMessage => AIMessageConsts.EstimateQuizTokensSuccess;
    protected override string ErrorMessage => AIMessageConsts.EstimateQuizTokensError;

    protected override async Task<EstimateQuizTokensDto> ExecuteAsync(EstimateQuizTokensQuery query, CancellationToken ct)
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
        var dailyTokensUsed = await tokenUsageRepo.GetDailyTokenUsageAsync(query.UserId, DateTime.UtcNow, ct);

        var inputTokens = await CalculateInputTokensAsync(query.CourseId, ct);

        var estimatedOutputTokens = Math.Max(1, query.QuestionCount) * EstimatedTokensPerOutputQuestion;
        var totalEstimatedTokens = inputTokens + estimatedOutputTokens;

        var estimatedPercentage = Math.Round((double)totalEstimatedTokens / dailyTokenLimit * 100, 2);
        var remainingDailyTokens = Math.Max(0, dailyTokenLimit - dailyTokensUsed);
        var canGenerate = (dailyTokensUsed + totalEstimatedTokens) <= dailyTokenLimit;

        return new EstimateQuizTokensDto
        {
            EstimatedTokens = totalEstimatedTokens,
            DailyTokenLimit = dailyTokenLimit,
            DailyTokensUsed = dailyTokensUsed,
            EstimatedDailyUsagePercentage = estimatedPercentage,
            RemainingDailyTokens = remainingDailyTokens,
            CanGenerate = canGenerate
        };
    }

    private async Task<int> CalculateInputTokensAsync(Guid courseId, CancellationToken ct)
    {
        var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
        var resources = await resourceRepo.Find(r => r.CourseId == courseId).ToListAsync(ct);

        var updatedAny = false;
        var inputTokens = 0;

        foreach (var resource in resources)
        {
            if (resource.EstimatedTokens <= 0)
            {
                await using var stream = await fileService.DownloadAsync($"{courseId}/{resource.Name}", _containerName, ct);
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

            inputTokens += resource.EstimatedTokens;
        }

        if (updatedAny)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }

        return inputTokens;
    }
}
