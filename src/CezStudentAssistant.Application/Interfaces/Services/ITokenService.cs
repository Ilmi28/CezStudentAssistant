namespace CezStudentAssistant.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId);
    Task<string> RotateRefreshTokenAsync(Guid userId, CancellationToken ct);
    bool TryGetGracePeriodToken(string token, out (string NewRefreshToken, Guid UserId) graceTokenInfo);
}
