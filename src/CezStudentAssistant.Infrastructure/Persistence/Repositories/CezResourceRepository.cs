using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;

namespace CezStudentAssistant.Infrastructure.Persistence.Repositories;

public class CezResourceRepository : GenericRepository<CezResource>, ICezResourceRepository
{
    public CezResourceRepository(AppDbContext context) : base(context)
    {
    }
}
