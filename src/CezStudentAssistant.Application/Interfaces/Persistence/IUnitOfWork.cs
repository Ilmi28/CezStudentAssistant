namespace CezStudentAssistant.Application.Interfaces.Persistence;

public interface IUnitOfWork : IDisposable
{
    TRepository Repository<TRepository>()
        where TRepository : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

