using CezStudentAssistant.Application.Responses;

namespace CezStudentAssistant.Application.Interfaces.CQRS;

/// <summary>
/// Generic interface for handling queries of a specified type and returning a response asynchronously.
/// </summary>
/// <typeparam name="TQuery">Type of the specific query.</typeparam>
/// <typeparam name="TResponse">Type of the specific response.</typeparam>
public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery
{
    /// <summary>
    /// Handles the specified query asynchronously and returns a response of the specified type.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response of the specified type.</returns>
    Task<ApiResponse<TResponse>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
