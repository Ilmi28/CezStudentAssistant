using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;

namespace CezStudentAssistant.Application.CommandHandlers;

public class LoginWithCezCommandHandler(ICezApiClient cezApiClient) : BaseCommandHandler<LoginWithCezCommand, CezLoginResponse>
{
    protected override ApiMessage SuccessMessage => new ApiMessage(this, "Successfully logged in with CEZ.");

    protected override ApiMessage ErrorMessage => new ApiMessage(this, "Failed to log in with CEZ.");

    protected override async Task<CezLoginResponse> ExecuteAsync(LoginWithCezCommand command, CancellationToken ct)
    {
        var loginResponse = await cezApiClient.LoginToCez(new Requests.Cez.CezLoginRequest
        {
            UserName = command.UserName,
            Password = command.Password,
        });
        return loginResponse;
    }
}
