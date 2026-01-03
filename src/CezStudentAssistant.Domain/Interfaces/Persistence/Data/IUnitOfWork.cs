namespace CezStudentAssistant.Domain.Interfaces.Persistence.Data;

public interface IUnitOfWork : IDisposable
{
    TRepository Repository<TRepository>()
        where TRepository : class;
}
