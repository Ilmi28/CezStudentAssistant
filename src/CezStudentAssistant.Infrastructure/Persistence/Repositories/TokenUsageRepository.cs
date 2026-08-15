using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Infrastructure.Persistence.Repositories;


public class TokenUsageRepository : GenericRepository<TokenUsage>, ITokenUsageRepository
{
    public TokenUsageRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<int> GetDailyTokenUsageAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _dbSet
            .Where(x => x.UserId == userId && x.CreatedAt >= startOfDay && x.CreatedAt < endOfDay)
            .SumAsync(x => x.UsageCount, cancellationToken);
    }
}

