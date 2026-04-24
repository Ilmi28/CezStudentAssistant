using CezStudentAssistant.Application.CommandHandlers;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Exceptions;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Responses;
using FluentValidation;

namespace CezStudentAssistant.Domain.CommandHandlers;

public abstract class ValidatableCommandHandler<TCommand>(IValidator<TCommand> validator)
    : BaseCommandHandler<TCommand>
    where TCommand : ICommand
{
    protected abstract ApiMessage ValidationMessage { get; }

    public override async Task<ApiResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(command, cancellationToken);
        if (!result.IsValid)
            throw new ApiValidationException(ValidationMessage, result.Errors);

        return await base.HandleAsync(command, cancellationToken);
    }
}

public abstract class ValidatableCommandHandler<TCommand, TResponse>(IValidator<TCommand> validator)
    : BaseCommandHandler<TCommand, TResponse>
    where TCommand : ICommand
{
    protected abstract ApiMessage ValidationMessage { get; }

    public override async Task<ApiResponse<TResponse>> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(command, cancellationToken);
        if (!result.IsValid)
            throw new ApiValidationException(ValidationMessage, result.Errors);

        return await base.HandleAsync(command, cancellationToken);
    }
}