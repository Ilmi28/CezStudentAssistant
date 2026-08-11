using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Interfaces.Repositories;

namespace CezStudentAssistant.Application.Commands.Auth;

public sealed record RefreshTokenCommand(string? RefreshToken = null) : ICommand { }

public class RefreshTokenCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ITokenService tokenService) : BaseCommandHandler<RefreshTokenCommand>
{
    protected override string SuccessMessage => AuthMessagesConsts.RefreshSuccess;

    protected override string ErrorMessage => AuthMessagesConsts.RefreshError;

    protected override async Task ExecuteAsync(RefreshTokenCommand command, CancellationToken ct)
    {
        var refreshToken = !string.IsNullOrWhiteSpace(command.RefreshToken)
            ? command.RefreshToken
            : currentUserService.GetRefreshToken();

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedException(AuthMessagesConsts.InvalidRefreshToken);

        var tokenRepo = unitOfWork.Repository<IRefreshTokenRepository>();
        var existingToken = await tokenRepo.GetSingleAsync(x => x.Token == refreshToken, ct);

        if (existingToken is null || existingToken.ExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedException(AuthMessagesConsts.InvalidRefreshToken);

        var newRefreshToken = await tokenService.RotateRefreshTokenAsync(existingToken.UserId, ct);
        var accessToken = tokenService.GenerateAccessToken(existingToken.UserId);

        currentUserService.SetSession(accessToken, newRefreshToken);
    }
}
