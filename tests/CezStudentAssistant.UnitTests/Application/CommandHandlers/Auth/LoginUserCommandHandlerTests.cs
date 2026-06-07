using CezStudentAssistant.Application.CommandHandlers.Auth;
using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Domain.Interfaces.Services;
using FluentAssertions;
using NSubstitute;
using System.Linq.Expressions;

namespace CezStudentAssistant.UnitTests.Application.CommandHandlers.Auth;

public class LoginUserCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IPasswordService _passwordService = null!;
    private ITokenService _tokenService = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUserRepository _userRepository = null!;
    private LoginUserCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _passwordService = Substitute.For<IPasswordService>();
        _tokenService = Substitute.For<ITokenService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _userRepository = Substitute.For<IUserRepository>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);

        _sut = new LoginUserCommandHandler(_unitOfWork, _passwordService, _currentUserService, _tokenService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task Handle_ShouldSetSession_WhenCredentialsAreValid()
    {
        var command = new LoginUserCommand { UserName = "testuser", Password = "password123" };
        var userId = Guid.NewGuid();
        var user = new User { UserName = "testuser", PasswordHash = "hashedPassword" };
        user.GetType().GetProperty("Id")?.SetValue(user, userId);

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordService.VerifyPassword(command.Password, user.PasswordHash).Returns(true);
        _tokenService.HandleRefreshToken(userId, Arg.Any<CancellationToken>())
            .Returns("refresh-token");
        _tokenService.GenerateAccessToken(userId).Returns("access-token");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        _currentUserService.Received(1).SetSession("access-token", "refresh-token");
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserDoesNotExist()
    {
        var command = new LoginUserCommand { UserName = "nonexistent", Password = "password123" };

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenPasswordIsIncorrect()
    {
        var command = new LoginUserCommand { UserName = "testuser", Password = "wrongpassword" };
        var user = new User { UserName = "testuser", PasswordHash = "hashedPassword" };

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordService.VerifyPassword(command.Password, user.PasswordHash).Returns(false);

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
