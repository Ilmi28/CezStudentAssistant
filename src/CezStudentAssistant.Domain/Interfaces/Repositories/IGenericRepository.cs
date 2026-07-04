using CezStudentAssistant.Domain.Entities;
using System.Linq.Expressions;

namespace CezStudentAssistant.Domain.Interfaces.Repositories;

/// <summary>
/// Defines a generic repository interface for performing standard data access operations on entities of type TEntity.
/// </summary>
/// <remarks>This interface provides a common abstraction for CRUD (Create, Read, Update, Delete) operations on
/// entities, enabling consistent data access patterns across different entity types. Implementations typically interact
/// with a data store such as a database. All operations are asynchronous to support non-blocking data access.</remarks>
/// <typeparam name="TEntity">The type of entity managed by the repository. Must implement the IBaseEntity interface.</typeparam>
public interface IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Synchronously retrieves a queryable collection of all entities of type TEntity from the data source.
    /// </summary>
    /// <remarks>Use the includes parameter to specify related data that should be loaded along with the main
    /// entities. If no includes are specified, only the main entities are retrieved.</remarks>
    /// <param name="asNoTracking">A boolean value indicating whether the entities should be tracked by the context.</param>
    /// <param name="includes">An array of expressions specifying related entities to include in the query results. Each expression identifies
    /// a navigation property to be eagerly loaded.</param>
    /// <returns>An IQueryable collection of all entities of type TEntity.</returns>
    IQueryable<TEntity> GetAll(bool asNoTracking = false, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Synchronously retrieves a queryable collection of entities that satisfy the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression that defines the conditions the returned entities must satisfy.</param>
    /// <param name="asNoTracking">A boolean value indicating whether the entities should be tracked by the context.</param>
    /// <param name="includes">One or more expressions specifying related entities to include in the query results. Use to eagerly load
    /// navigation properties.</param>
    /// <returns>An IQueryable collection of entities that match the specified predicate.</returns>
    IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = false, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Asynchronously determines whether any entities satisfy the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression that defines the conditions to test each entity for a match.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if any
    /// entities match the predicate; otherwise, <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves an entity by its unique identifier, optionally including related entities.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <param name="asNoTracking">A boolean value indicating whether the entities should be tracked by the context.</param>
    /// <param name="includes">An array of expressions specifying related entities to include in the query. Each expression identifies a
    /// navigation property to be eagerly loaded.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity matching the specified
    /// identifier, or <see langword="null"/> if no entity is found.</returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default, bool asNoTracking = false, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Asynchronously adds the specified entity to the data store.
    /// </summary>
    /// <param name="entity">The entity to add to the data store. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously updates the specified entity in the data store.
    /// </summary>
    /// <param name="entity">The entity to update. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the update operation.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes the specified entity from the data store.
    /// </summary>
    /// <param name="entity">The entity to delete. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves a single entity that satisfies the specified predicate, optionally including related entities.
    /// </summary>
    /// <param name="predicate">An expression that defines the conditions the returned entity must satisfy.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <param name="asNoTracking">A boolean value indicating whether the entity should be tracked by the context.</param>
    /// <param name="includes">An array of expressions specifying related entities to include in the query. Each expression identifies a
    /// navigation property to be eagerly loaded.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity matching the specified
    /// predicate, or <see langword="null"/> if no entity is found.</returns>
    Task<TEntity?> GetSingleAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        bool asNoTracking = false,
        params Expression<Func<TEntity, object>>[] includes);
}
