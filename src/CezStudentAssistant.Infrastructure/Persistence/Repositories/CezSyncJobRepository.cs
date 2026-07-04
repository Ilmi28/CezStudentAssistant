using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;

namespace CezStudentAssistant.Infrastructure.Persistence.Repositories;

public class CezSyncJobRepository : GenericRepository<CezSyncJob>, ICezSyncJobRepository
{
    public CezSyncJobRepository(AppDbContext context) : base(context)
    {
    }
}
