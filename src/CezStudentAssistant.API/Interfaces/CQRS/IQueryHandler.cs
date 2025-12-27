namespace CezStudentAssistant.API.Interfaces.CQRS
{
    /// <summary>
    /// Defines a handler for processing queries and returning a response of a specified type.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to be handled. Must implement the IQuery interface.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by the handler. Must be a reference type.</typeparam>
    public interface IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery
        where TResponse : class
    {
        /// <summary>
        /// Handles the specified query asynchronously and returns a response of the specified type.
        /// </summary>
        /// <param name="query">The query to be handled.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response of the specified type.</returns>
        Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}
