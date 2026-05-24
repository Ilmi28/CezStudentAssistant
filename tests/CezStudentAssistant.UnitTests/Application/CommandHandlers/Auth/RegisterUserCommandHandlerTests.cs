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

public class RegisterUserCommandHandlerTests
{
    private IValidator<RegisterUserCommand> _validator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IPasswordService _passwordService = null!;
    private IUserRepository _userRepository = null!;
    private RegisterUserCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = Substitute.For<IValidator<RegisterUserCommand>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _passwordService = Substitute.For<IPasswordService>();
        _userRepository = Substitute.For<IUserRepository>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);

        _sut = new RegisterUserCommandHandler(_validator, _unitOfWork, _passwordService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task HandleAsync_ShouldReturnUserId_WhenDataIsValid()
    {
        // Arrange
        var command = new RegisterUserCommand { UserName = "newuser", Password = "Password123!" };
        
        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _userRepository.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordService.CreatePasswordHash(command.Password).Returns("hashedPassword");

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Success.Should().BeTrue();
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => u.UserName == command.UserName), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task HandleAsync_ShouldThrowConflictException_WhenUsernameAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand { UserName = "existinguser", Password = "Password123!" };

        _validator.ValidateAsync(command, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _userRepository.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Func<Task> act = () => _sut.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }
}
