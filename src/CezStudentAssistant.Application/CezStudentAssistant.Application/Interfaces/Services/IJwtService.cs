using CezStudentAssistant.Application.Dtos.Auth;
using System.Security.Claims;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface IJwtService
{
    Task<AuthTokens> GenerateTokensAsync(Guid userId, string userName, CancellationToken ct = default);
    Task<AuthTokens> RefreshTokensAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
