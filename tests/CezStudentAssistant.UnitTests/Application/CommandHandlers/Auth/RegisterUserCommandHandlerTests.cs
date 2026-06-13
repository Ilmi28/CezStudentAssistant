using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using System.Linq.Expressions;

namespace CezStudentAssistant.UnitTests.Application.CommandHandlers.Auth;

public class RegisterUserCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IPasswordService _passwordService = null!;
    private IUserRepository _userRepository = null!;
    private RegisterUserCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _passwordService = Substitute.For<IPasswordService>();
        _userRepository = Substitute.For<IUserRepository>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);

        _sut = new RegisterUserCommandHandler(_unitOfWork, _passwordService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnSuccessResponse_WhenDataIsValid()
    {
        var command = new RegisterUserCommand("newuser", "Password123!");
        
        _userRepository.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordService.CreatePasswordHash(command.Password).Returns("hashedPassword");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => u.UserName == command.UserName), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task Handle_ShouldThrowConflictException_WhenUsernameAlreadyExists()
    {
        var command = new RegisterUserCommand("existinguser", "Password123!");

        _userRepository.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
