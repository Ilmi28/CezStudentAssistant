using CezStudentAssistant.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
        _sut = new CurrentUserService(_httpContextAccessor);
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
    public void SetSession_ShouldAppendCookies()
    {
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        _sut.SetSession("access-token", "refresh-token");

        var setCookieHeader = httpContext.Response.Headers["Set-Cookie"].ToString();
        setCookieHeader.Should().Contain("accessToken=");
        setCookieHeader.Should().Contain("refreshToken=");
    }
}
