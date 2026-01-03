using System.Linq.Expressions;
using CezStudentAssistant.Domain.Interfaces.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace CezStudentAssistant.Infrastructure.Persistence.Data;

public class GenericRepository<TEntity>(DbContext context) : IGenericRepository<TEntity>
    where TEntity : class, IBaseEntity
{
    protected readonly DbContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.LastModifiedAt = DateTime.UtcNow;

        _dbSet.Update(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.DeletedAt = DateTime.UtcNow;

        _dbSet.Update(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes
    )
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        query = query.Where(predicate);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes
    )
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes
    )
    {
        if (includes == null || includes.Length == 0)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        IQueryable<TEntity> query = _dbSet.AsQueryable();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
