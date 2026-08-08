using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;

namespace CezStudentAssistant.Infrastructure.Persistence.Repositories;

public class SelectedQuizOptionRepository : GenericRepository<SelectedQuizOption>, ISelectedQuizOptionRepository
{
    public SelectedQuizOptionRepository(AppDbContext context) : base(context)
    {
    }
}
