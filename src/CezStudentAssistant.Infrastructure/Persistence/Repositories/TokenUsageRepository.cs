using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;

namespace CezStudentAssistant.Infrastructure.Persistence.Repositories;

public class TokenUsageRepository : GenericRepository<TokenUsage>, ITokenUsageRepository
{
    public TokenUsageRepository(AppDbContext context) : base(context)
    {
    }
}
