using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;

namespace CezStudentAssistant.Application.CommandHandlers.Auth;

public class LoginWithCezCommandHandler(
    ICezService cezService,
    ITokenService tokenService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : BaseCommandHandler<LoginWithCezCommand>
{
    protected override ApiMessage SuccessMessage => new ApiMessage(this, CezMessagesConsts.LoginSuccess);

    protected override ApiMessage ErrorMessage => new ApiMessage(this, CezMessagesConsts.LoginError);

    protected override async Task ExecuteAsync(LoginWithCezCommand command, CancellationToken ct)
    {
        var userId = await cezService.LoginWithCezAsync(command.UserName, command.Password, ct);

        var refreshToken = await tokenService.HandleRefreshToken(userId, ct);
        var accessToken = tokenService.GenerateAccessToken(userId);
        await unitOfWork.SaveChangesAsync(ct);

        currentUserService.SetSession(accessToken, refreshToken);
    }
}
