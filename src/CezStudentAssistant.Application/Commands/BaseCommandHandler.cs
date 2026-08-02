using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Responses;
using MediatR;

namespace CezStudentAssistant.Application.Commands;

public abstract class BaseCommandHandler<TCommand> : IRequestHandler<TCommand, ApiResponse>
    where TCommand : ICommand
{
    protected abstract string SuccessMessage { get; }

    protected abstract string ErrorMessage { get; }

    protected abstract Task ExecuteAsync(TCommand command, CancellationToken ct);


    public async virtual Task<ApiResponse> Handle(TCommand command, CancellationToken cancellationToken)
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

public abstract class BaseCommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, ApiResponse<TResponse>>
    where TCommand : ICommand<TResponse>
{
    protected abstract string SuccessMessage { get; }

    protected abstract string ErrorMessage { get; }

    protected abstract Task<TResponse> ExecuteAsync(TCommand command, CancellationToken ct);

    public async virtual Task<ApiResponse<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
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
