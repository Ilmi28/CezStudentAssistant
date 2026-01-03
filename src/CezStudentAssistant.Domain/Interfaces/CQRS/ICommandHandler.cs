using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Domain.Interfaces.CQRS;

/// <summary>
/// Defines a contract for handling a command of a specified type and returning a response asynchronously.
/// </summary>
/// <typeparam name="TCommand">The type of command to be handled. Must implement the <see cref="ICommand"/> interface.</typeparam>
/// <typeparam name="TResponse">The type of response returned after handling the command. Must be a reference type.</typeparam>
public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand
    where TResponse : class
{
    /// <summary>
    /// Asynchronously processes the specified command and returns an API response containing the result.
    /// </summary>
    /// <param name="command">The command to be handled. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see
    /// cref="ApiResponse{TResponse}"/> with the outcome of the command.</returns>
    Task<ApiResponse<TResponse>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Defines a handler for processing commands of a specified type asynchronously.
/// </summary>
/// <typeparam name="TCommand">The type of command to be handled. Must implement the <see cref="ICommand"/> interface.</typeparam>
public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Asynchronously handles the specified command.
    /// </summary>
    /// <param name="command">The command to be processed. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response produced by
    /// handling the command.</returns>
    Task<ApiResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
