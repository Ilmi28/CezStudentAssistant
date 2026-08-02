using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Notifications;
using CezStudentAssistant.Application.Responses;
using MediatR;

namespace CezStudentAssistant.Application.Commands.Auth;

public sealed record LoginWithCezCommand(string UserName, string Password) : ICommand { }

public class LoginWithCezCommandHandler(
    ICezService cezService,
    ITokenService tokenService,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IPublisher publisher) : BaseCommandHandler<LoginWithCezCommand>
{
    protected override string SuccessMessage => CezMessagesConsts.LoginSuccess;

    protected override string ErrorMessage => CezMessagesConsts.LoginError;

    protected override async Task ExecuteAsync(LoginWithCezCommand command, CancellationToken ct)
    {
        var userId = await cezService.LoginWithCezAsync(command.UserName, command.Password, ct);

        var refreshToken = await tokenService.HandleRefreshToken(userId, ct);
        var accessToken = tokenService.GenerateAccessToken(userId);
        await unitOfWork.SaveChangesAsync(ct);
        await publisher.Publish(new CezLoginSucceededNotification(userId), ct);

        currentUserService.SetSession(accessToken, refreshToken);
    }
}
