using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using System.Linq.Expressions;

namespace CezStudentAssistant.UnitTests.Application.Commands.Auth;

public class RefreshTokenCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private ITokenService _tokenService = null!;
    private ICurrentUserService _currentUserService = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private RefreshTokenCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tokenService = Substitute.For<ITokenService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

        _unitOfWork.Repository<IRefreshTokenRepository>().Returns(_refreshTokenRepository);

        _sut = new RefreshTokenCommandHandler(_unitOfWork, _currentUserService, _tokenService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task Handle_ShouldSetSession_WhenRefreshTokenIsValid()
    {
        var userId = Guid.NewGuid();
        var validToken = "valid-refresh-token";
        var command = new RefreshTokenCommand(validToken);

        var tokenEntity = new RefreshToken
        {
            Token = validToken,
            ExpiryTime = DateTime.UtcNow.AddDays(7),
            UserId = userId
        };

        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(tokenEntity);

        _tokenService.RotateRefreshTokenAsync(userId, Arg.Any<CancellationToken>())
            .Returns("new-refresh-token");
        _tokenService.GenerateAccessToken(userId)
            .Returns("new-access-token");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        _currentUserService.Received(1).SetSession("new-access-token", "new-refresh-token");
    }

    [Test]
    public async Task Handle_ShouldSetSession_WhenCommandTokenIsNullButCookieExists()
    {
        var userId = Guid.NewGuid();
        var cookieToken = "cookie-refresh-token";
        var command = new RefreshTokenCommand(null);

        _currentUserService.GetRefreshToken().Returns(cookieToken);

        var tokenEntity = new RefreshToken
        {
            Token = cookieToken,
            ExpiryTime = DateTime.UtcNow.AddDays(7),
            UserId = userId
        };

        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(tokenEntity);

        _tokenService.RotateRefreshTokenAsync(userId, Arg.Any<CancellationToken>())
            .Returns("new-refresh-token");
        _tokenService.GenerateAccessToken(userId)
            .Returns("new-access-token");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        _currentUserService.Received(1).SetSession("new-access-token", "new-refresh-token");
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenRefreshTokenNullOrEmpty()
    {
        var command = new RefreshTokenCommand(string.Empty);

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenRefreshTokenDoesNotExistInDb()
    {
        var command = new RefreshTokenCommand("nonexistent-token");

        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test]
    public async Task Handle_ShouldSetSession_WhenTokenFoundInGracePeriodCache()
    {
        var userId = Guid.NewGuid();
        var oldToken = "old-token-in-grace-period";
        var command = new RefreshTokenCommand(oldToken);

        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        (string NewRefreshToken, Guid UserId) graceInfo = ("new-token-from-grace", userId);
        _tokenService.TryGetGracePeriodToken(oldToken, out Arg.Any<(string, Guid)>())
            .Returns(x =>
            {
                x[1] = graceInfo;
                return true;
            });

        _tokenService.GenerateAccessToken(userId).Returns("grace-access-token");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        _currentUserService.Received(1).SetSession("grace-access-token", "new-token-from-grace");
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenRefreshTokenIsExpired()
    {
        var validToken = "expired-token";
        var command = new RefreshTokenCommand(validToken);

        var tokenEntity = new RefreshToken
        {
            Token = validToken,
            ExpiryTime = DateTime.UtcNow.AddMinutes(-5),
            UserId = Guid.NewGuid()
        };

        _refreshTokenRepository.GetSingleAsync(Arg.Any<Expression<Func<RefreshToken, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(tokenEntity);

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
