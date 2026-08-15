using CezStudentAssistant.Domain.Entities;

namespace CezStudentAssistant.Domain.Interfaces.Repositories;

public interface ITokenUsageRepository : IGenericRepository<TokenUsage>
{
    Task<int> GetDailyTokenUsageAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default);
}

