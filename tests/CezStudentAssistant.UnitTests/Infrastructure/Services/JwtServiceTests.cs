using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Services;
using CezStudentAssistant.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Linq.Expressions;

namespace CezStudentAssistant.UnitTests.Infrastructure.Services;

public class JwtServiceTests
{
    private IOptions<JwtSettings> _jwtOptions = null!;
    private IServiceProvider _serviceProvider = null!;
    private IServiceScope _serviceScope = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private JwtService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _jwtOptions = Options.Create(new JwtSettings
        {
            Secret = "SuperSecretKeyForDevelopmentAndTestingNeedsToBeAtLeast32BytesLong!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 30
        });

        _serviceProvider = Substitute.For<IServiceProvider>();
        _serviceScope = Substitute.For<IServiceScope>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);
        scopeFactory.CreateScope().Returns(_serviceScope);
        _serviceScope.ServiceProvider.Returns(_serviceProvider);

        _serviceProvider.GetService(typeof(IUnitOfWork)).Returns(_unitOfWork);
        _unitOfWork.Repository<IRefreshTokenRepository>().Returns(_refreshTokenRepository);

        _sut = new JwtService(_jwtOptions, _serviceProvider);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceScope.Dispose();
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task GenerateTokensAsync_ShouldCreateNewToken_WhenNoneExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        // Act
        var result = await _sut.GenerateTokensAsync(userId, userName);

        // Assert
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Is<RefreshToken>(t => t.UserId == userId), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GenerateTokensAsync_ShouldUpdateExistingToken_WhenOneExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userName = "testuser";
        var existingToken = new RefreshToken { UserId = userId, Token = "old", ExpiryTime = DateTime.UtcNow };
        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingToken);

        // Act
        var result = await _sut.GenerateTokensAsync(userId, userName);

        // Assert
        result.RefreshToken.Should().NotBe("old");
        await _refreshTokenRepository.Received(1).UpdateAsync(existingToken, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RevokeTokenAsync_ShouldDeleteToken_WhenTokenExists()
    {
        // Arrange
        var tokenValue = "someToken";
        var token = new RefreshToken { Token = tokenValue, ExpiryTime = DateTime.UtcNow, UserId = Guid.NewGuid() };
        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(token);

        // Act
        await _sut.RevokeTokenAsync(tokenValue);

        // Assert
        await _refreshTokenRepository.Received(1).DeleteAsync(token, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RefreshTokensAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var tokenValue = "validToken";
        var userId = Guid.NewGuid();
        var user = new User { UserName = "testuser" };
        var refreshToken = new RefreshToken { Token = tokenValue, UserId = userId, User = user, ExpiryTime = DateTime.UtcNow.AddDays(1) };
        
        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>(), Arg.Any<Expression<Func<RefreshToken, object>>>())
            .Returns(refreshToken);

        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(refreshToken);

        // Act
        var result = await _sut.RefreshTokensAsync(tokenValue);

        // Assert
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(tokenValue);
    }
}
