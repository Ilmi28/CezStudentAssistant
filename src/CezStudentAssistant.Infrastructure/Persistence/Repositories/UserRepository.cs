using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Persistence.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;

namespace CezStudentAssistant.Infrastructure.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }
}
