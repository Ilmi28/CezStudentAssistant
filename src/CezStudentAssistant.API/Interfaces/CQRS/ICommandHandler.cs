namespace CezStudentAssistant.API.Interfaces.CQRS
{
    /// <summary>
    /// Defines a handler for processing a command and returning a response of a specified type.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be handled. Must implement the <see cref="ICommand"/> interface.</typeparam>
    /// <typeparam name="TResponse">The type of response returned after handling the command. Must be a reference type.</typeparam>
    public interface ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand
        where TResponse : class
    {
        /// <summary>
        /// Asynchronously handles the specified command and returns a response of the corresponding type.
        /// </summary>
        /// <param name="command">The command to be processed. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response produced by
        /// handling the command.</returns>
        Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}
