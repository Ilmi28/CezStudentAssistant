using CezStudentAssistant.Application.Interfaces.Common;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace CezStudentAssistant.Infrastructure.Services;

public class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    IOptions<JwtSettings> jwtOptions)
    : ICurrentUserService, IScopedService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
    private HttpResponse Response => httpContextAccessor.HttpContext.Response;
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    private CookieOptions CreateCookieOptions(DateTime expires) => new()
    {
        HttpOnly = true,
        Secure = httpContextAccessor.HttpContext?.Request.IsHttps ?? false,
        SameSite = SameSiteMode.None,
        Path = "/",
        Expires = expires
    };

    public Guid? GetCurrentUserId() => Guid.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    public string? GetRefreshToken() => httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];

    public void SetSession(string accessToken, string refreshToken)
    {
        Response.Cookies.Append("accessToken", accessToken, CreateCookieOptions(DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes)));
        Response.Cookies.Append("refreshToken", refreshToken, CreateCookieOptions(DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)));
    }

    public void ClearSession()
    {
        var options = CreateCookieOptions(DateTime.UtcNow.AddDays(-1));
        Response.Cookies.Delete("accessToken", options);
        Response.Cookies.Delete("refreshToken", options);
    }
}
