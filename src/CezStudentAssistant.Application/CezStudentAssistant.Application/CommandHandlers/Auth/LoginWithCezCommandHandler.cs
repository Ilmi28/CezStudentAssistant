using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Domain.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Domain.CommandHandlers;
using FluentValidation;

namespace CezStudentAssistant.Application.CommandHandlers.Auth;

public class LoginWithCezCommandHandler : ValidatableCommandHandler<LoginWithCezCommand, CezLoginResponse>
{
    private readonly ICezAuthService _cezAuthService;
    public LoginWithCezCommandHandler(
        IValidator<LoginWithCezCommand> validator,
        ICezAuthService cezAuthService) : base(validator)
    {
        _cezAuthService = cezAuthService;
    }

    protected override ApiMessage SuccessMessage => new ApiMessage(this, CezMessagesConsts.LoginSuccess);

    protected override ApiMessage ErrorMessage => new ApiMessage(this, CezMessagesConsts.LoginError);

    protected override ApiMessage ValidationMessage => new ApiMessage(this, CezMessagesConsts.LoginValidationError);
    protected override async Task<CezLoginResponse> ExecuteAsync(LoginWithCezCommand command, CancellationToken ct)
    {
        return await _cezAuthService.LoginWithCezAsync(command.UserName, command.Password, command, ct);
    }
}
