using CezStudentAssistant.Infrastructure.Services;
using CezStudentAssistant.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Security.Claims;

namespace CezStudentAssistant.UnitTests.Infrastructure.Services;

public class CurrentUserServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CurrentUserService _sut;

    public CurrentUserServiceTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var jwtSettings = Options.Create(new JwtSettings 
        { 
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 30 
        });
        _sut = new CurrentUserService(_httpContextAccessor, jwtSettings);
    }

    [Test]
    public void GetCurrentUserId_ShouldReturnGuid_WhenClaimExists()
    {
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var result = _sut.GetCurrentUserId();

        result.Should().Be(userId);
    }

    [Test]
    public void GetCurrentUserId_ShouldReturnNull_WhenClaimDoesNotExist()
    {
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var result = _sut.GetCurrentUserId();

        result.Should().BeNull();
    }

    [Test]
    public void SetSession_ShouldAppendCookiesWithRespectiveExpirations()
    {
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        _sut.SetSession("access-token", "refresh-token");

        var setCookieHeaders = httpContext.Response.Headers["Set-Cookie"].ToList();
        var accessTokenCookie = setCookieHeaders.FirstOrDefault(c => c != null && c.StartsWith("accessToken="));
        var refreshTokenCookie = setCookieHeaders.FirstOrDefault(c => c != null && c.StartsWith("refreshToken="));

        accessTokenCookie.Should().NotBeNull();
        refreshTokenCookie.Should().NotBeNull();
        accessTokenCookie.Should().Contain("expires=");
        refreshTokenCookie.Should().Contain("expires=");
    }

    [Test]
    public void GetRefreshToken_ShouldReturnToken_WhenCookieExists()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Cookie"] = "refreshToken=my-refresh-token";
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var result = _sut.GetRefreshToken();

        result.Should().Be("my-refresh-token");
    }

    [Test]
    public void GetRefreshToken_ShouldReturnNull_WhenCookieDoesNotExist()
    {
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var result = _sut.GetRefreshToken();

        result.Should().BeNull();
    }
}
