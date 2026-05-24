using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Responses;

namespace CezStudentAssistant.Application.CommandHandlers;

public abstract class BaseCommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    protected abstract ApiMessage SuccessMessage { get; }

    protected abstract ApiMessage ErrorMessage { get; }

    protected abstract Task ExecuteAsync(TCommand command, CancellationToken ct);


    public async virtual Task<ApiResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await ExecuteAsync(command, cancellationToken);

            return new SuccessResponse(SuccessMessage);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            throw new AppException(ErrorMessage, ex);
        }
    }
}

public abstract class BaseCommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand
{
    protected abstract ApiMessage SuccessMessage { get; }

    protected abstract ApiMessage ErrorMessage { get; }

    protected abstract Task<TResponse> ExecuteAsync(TCommand command, CancellationToken ct);

    public async virtual Task<ApiResponse<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            TResponse result = await ExecuteAsync(command, cancellationToken);

            return new SuccessResponse<TResponse>(SuccessMessage, result);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            throw new AppException(ErrorMessage, ex);
        }
    }
}
