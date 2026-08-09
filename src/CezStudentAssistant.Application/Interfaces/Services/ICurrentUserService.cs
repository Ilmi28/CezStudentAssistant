namespace CezStudentAssistant.Application.Interfaces.Services;

/// <summary>
/// Provides information about the current user, including their identity and authentication status.
/// </summary>
/// <remarks>This interface is typically used in applications to access user-specific data, such as user ID and
/// username, and to determine if the user is authenticated.</remarks>
public interface ICurrentUserService
{
    Guid? GetCurrentUserId();
    void SetSession(string accessToken, string refreshToken);
    void ClearSession();
}
