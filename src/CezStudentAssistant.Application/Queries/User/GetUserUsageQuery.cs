using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.User;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Queries.User;

public sealed class GetUserUsageQuery : IQuery<UserUsageDto>, IUserRequest
{
    public Guid UserId { get; set; }
}

public class GetUserUsageQueryHandler(IUnitOfWork unitOfWork, IConfiguration configuration)
    : BaseQueryHandler<GetUserUsageQuery, UserUsageDto>
{
    protected override string SuccessMessage => UserMessageConsts.GetUserUsageSuccess;
    protected override string ErrorMessage => UserMessageConsts.GetUserUsageError;

    protected override async Task<UserUsageDto> ExecuteAsync(GetUserUsageQuery query, CancellationToken ct)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var userExists = await userRepo.ExistsAsync(x => x.Id == query.UserId, ct);
        if (!userExists)
        {
            throw new NotFoundException(UserMessageConsts.UserNotFound);
        }

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
        var usagePercentage = Math.Round((double)totalUsedAndReserved / dailyTokenLimit * 100, 2);

        return new UserUsageDto
        {
            DailyTokensUsed = completedTokens,
            DailyTokensReserved = reservedTokens,
            DailyTokenLimit = dailyTokenLimit,
            DailyUsagePercentage = usagePercentage
        };
    }
}
