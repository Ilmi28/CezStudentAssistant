using CezStudentAssistant.Application.CommandHandlers.Auth;
using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Domain.Interfaces.Services;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using System.Linq.Expressions;

namespace CezStudentAssistant.UnitTests.Application.CommandHandlers.Auth;

public class LoginUserCommandHandlerTests
{
    private IValidator<LoginUserCommand> _validator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IPasswordService _passwordService = null!;
    private IUserRepository _userRepository = null!;
    private LoginUserCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = Substitute.For<IValidator<LoginUserCommand>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _passwordService = Substitute.For<IPasswordService>();
        _userRepository = Substitute.For<IUserRepository>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);

        _sut = new LoginUserCommandHandler(_validator, _unitOfWork, _passwordService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task HandleAsync_ShouldReturnUserId_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginUserCommand { UserName = "testuser", Password = "password123" };
        var userId = Guid.NewGuid();
        var user = new User { UserName = "testuser", PasswordHash = "hashedPassword" };
        user.GetType().GetProperty("Id")?.SetValue(user, userId);

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordService.VerifyPassword(command.Password, user.PasswordHash).Returns(true);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().Be(userId);
    }

    [Test]
    public async Task HandleAsync_ShouldThrowUnauthorizedException_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new LoginUserCommand { UserName = "nonexistent", Password = "password123" };

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        Func<Task> act = () => _sut.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test]
    public async Task HandleAsync_ShouldThrowUnauthorizedException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var command = new LoginUserCommand { UserName = "testuser", Password = "wrongpassword" };
        var user = new User { UserName = "testuser", PasswordHash = "hashedPassword" };

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordService.VerifyPassword(command.Password, user.PasswordHash).Returns(false);

        // Act
        Func<Task> act = () => _sut.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
