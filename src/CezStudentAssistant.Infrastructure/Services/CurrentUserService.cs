using CezStudentAssistant.Application.Interfaces.Common;
using CezStudentAssistant.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CezStudentAssistant.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService, IScopedService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
    private HttpResponse Response => httpContextAccessor.HttpContext.Response;

    private CookieOptions CookieOptions => new()
    {
        HttpOnly = true,
        Secure = httpContextAccessor.HttpContext?.Request.IsHttps ?? false,
        SameSite = SameSiteMode.Strict,
        Expires = DateTime.UtcNow.AddDays(7)
    };

    public Guid? GetCurrentUserId() => Guid.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    public void SetSession(string accessToken, string refreshToken)
    {
        Response.Cookies.Append("accessToken", accessToken, CookieOptions);
        Response.Cookies.Append("refreshToken", refreshToken, CookieOptions);
    }
}
