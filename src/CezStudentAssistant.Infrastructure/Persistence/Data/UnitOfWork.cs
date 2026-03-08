using CezStudentAssistant.Domain.Interfaces.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CezStudentAssistant.Infrastructure.Persistence.Data;

public class UnitOfWork(AppDbContext context, IServiceProvider serviceProvider) : IUnitOfWork
{
    public void Dispose()
    {
        context.Dispose();
        GC.SuppressFinalize(this);
    }

    public TRepository Repository<TRepository>()
        where TRepository : class
    {
        return serviceProvider.GetRequiredService<TRepository>();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
