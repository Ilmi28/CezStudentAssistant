namespace CezStudentAssistant.Application.Interfaces.Persistence;

/// <summary>
/// Defines a contract for a unit of work that coordinates the writing of changes and provides access to repositories
/// within a data context.
/// </summary>
/// <remarks>Implementations of this interface manage the lifetime and consistency of data operations, ensuring
/// that changes across multiple repositories are committed as a single transaction. The unit of work pattern helps
/// maintain data integrity and simplifies resource management. Instances should be disposed when no longer needed to
/// release underlying resources.</remarks>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Retrieves an instance of the specified repository type from the current context.
    /// </summary>
    /// <remarks>Use this method to access custom repositories registered with the context. The returned
    /// repository is typically managed by the underlying infrastructure and should not be disposed by the
    /// caller.</remarks>
    /// <typeparam name="TRepository">The type of repository to retrieve. Must be a reference type.</typeparam>
    /// <returns>An instance of the requested repository type. Returns null if the repository is not available.</returns>
    TRepository Repository<TRepository>()
        where TRepository : class;

    /// <summary>
    /// Asynchronously saves all changes made in this context to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous save operation.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries
    /// written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
