namespace CezStudentAssistant.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId);
    Task<string> HandleRefreshToken(Guid userId, CancellationToken ct);
}
