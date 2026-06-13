using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.Auth;

public sealed record LoginUserCommand(string UserName, string Password) : ICommand { }

public class LoginUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordService passwordService,
    ICurrentUserService currentUserService,
    ITokenService tokenService) : BaseCommandHandler<LoginUserCommand>
{
    protected override ApiMessage SuccessMessage => new ApiMessage(this, AuthMessagesConsts.LoginSuccess);

    protected override ApiMessage ErrorMessage => new ApiMessage(this, AuthMessagesConsts.LoginError);

    protected async override Task ExecuteAsync(LoginUserCommand command, CancellationToken ct)
    {
        var userRepo = unitOfWork.Repository<IUserRepository>();
        var user = await userRepo.GetSingleAsync(x => x.UserName == command.UserName, ct);

        if (user == null || user.PasswordHash == null || !passwordService.VerifyPassword(command.Password, user.PasswordHash))
            throw new UnauthorizedException(new ApiMessage(this, AuthMessagesConsts.LoginInvalidCredentials));

        var refreshToken = await tokenService.HandleRefreshToken(user.Id, ct);
        var accessToken = tokenService.GenerateAccessToken(user.Id);

        currentUserService.SetSession(accessToken, refreshToken);
    }
}
