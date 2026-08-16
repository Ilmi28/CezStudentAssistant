using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
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
    IConfiguration configuration) : BaseQueryHandler<EstimateQuizTokensQuery, EstimateQuizTokensDto>
{
    private const int EstimatedTokensPerOutputQuestion = 200;

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

        var resourceRepo = unitOfWork.Repository<ICezResourceRepository>();
        var inputTokens = await resourceRepo.Find(r => r.CourseId == query.CourseId)
            .SumAsync(r => r.EstimatedTokens, ct);

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
}
