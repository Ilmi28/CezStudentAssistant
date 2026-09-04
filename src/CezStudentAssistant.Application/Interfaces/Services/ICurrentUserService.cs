namespace CezStudentAssistant.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? GetCurrentUserId();
    string? GetRefreshToken();
    void SetSession(string accessToken, string refreshToken);
    void ClearSession();
}

