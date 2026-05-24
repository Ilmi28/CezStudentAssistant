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
    public void UserId_ShouldReturnGuid_WhenClaimExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.UserId;

        // Assert
        result.Should().Be(userId);
    }

    [Test]
    public void UserId_ShouldReturnNull_WhenClaimDoesNotExist()
    {
        // Arrange
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void IsAuthenticated_ShouldReturnTrue_WhenUserIsAuthenticated()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuth");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _sut.IsAuthenticated;

        // Assert
        result.Should().BeTrue();
    }
}
