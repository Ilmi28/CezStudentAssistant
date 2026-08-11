using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Services;
using CezStudentAssistant.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Linq.Expressions;

namespace CezStudentAssistant.UnitTests.Infrastructure.Services;

public class TokenServiceTests
{
    private IOptions<JwtSettings> _jwtOptions = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private TokenService _sut = null!;

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

        _unitOfWork = Substitute.For<IUnitOfWork>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

        _unitOfWork.Repository<IRefreshTokenRepository>().Returns(_refreshTokenRepository);

        _sut = new TokenService(_jwtOptions, _unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public void GenerateAccessToken_ShouldReturnToken()
    {
        var userId = Guid.NewGuid();

        var token = _sut.GenerateAccessToken(userId);

        token.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task RotateRefreshTokenAsync_ShouldCreateNewToken_WhenNoneExists()
    {
        var userId = Guid.NewGuid();
        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var refreshToken = await _sut.RotateRefreshTokenAsync(userId, CancellationToken.None);

        refreshToken.Should().NotBeNullOrWhiteSpace();
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Is<RefreshToken>(t => t.UserId == userId), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RotateRefreshTokenAsync_ShouldUpdateExistingToken_WhenOneExists()
    {
        var userId = Guid.NewGuid();
        var existingToken = new RefreshToken { UserId = userId, Token = "old", ExpiryTime = DateTime.UtcNow };
        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingToken);

        var refreshToken = await _sut.RotateRefreshTokenAsync(userId, CancellationToken.None);

        refreshToken.Should().NotBe("old");
        existingToken.Token.Should().NotBe("old");
        await _refreshTokenRepository.Received(1).UpdateAsync(existingToken, Arg.Any<CancellationToken>());
    }
}
