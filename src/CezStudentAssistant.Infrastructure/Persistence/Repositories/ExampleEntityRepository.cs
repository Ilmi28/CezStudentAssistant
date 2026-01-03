using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Persistence.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;

namespace CezStudentAssistant.Infrastructure.Persistence.Repositories;

public class ExampleEntityRepository : GenericRepository<ExampleEntity>, IExampleEntityRepository
{
    public ExampleEntityRepository(AppDbContext context)
        : base(context) { }
}
