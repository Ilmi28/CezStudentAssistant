using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Domain.CommandHandlers;
using FluentValidation;

namespace CezStudentAssistant.Application.CommandHandlers;

public class LoginWithCezCommandHandler : ValidatableCommandHandler<LoginWithCezCommand, CezLoginResponse>
{
    private readonly ICezApiClient _cezApiClient;
    public LoginWithCezCommandHandler(IValidator<LoginWithCezCommand> validator, ICezApiClient cezApiClient) : base(validator)
    {
        _cezApiClient = cezApiClient;
    }

    protected override ApiMessage SuccessMessage => new ApiMessage(this, "Successfully logged in with CEZ.");

    protected override ApiMessage ErrorMessage => new ApiMessage(this, "Failed to log in with CEZ.");

    protected override ApiMessage ValidationMessage => new ApiMessage(this, "Invalid login credentials for CEZ.");

    protected override async Task<CezLoginResponse> ExecuteAsync(LoginWithCezCommand command, CancellationToken ct)
    {
        var loginResponse = await _cezApiClient.LoginToCez(new Requests.Cez.CezLoginRequest
        {
            UserName = command.UserName,
            Password = command.Password,
        });
        return loginResponse;
    }
}
