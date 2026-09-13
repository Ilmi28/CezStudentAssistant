using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Commands.Auth;

[TestFixture]
public class ChangePasswordCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IUserRepository _userRepository = null!;
    private IPasswordService _passwordService = null!;
    private ChangePasswordCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _passwordService = Substitute.For<IPasswordService>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _sut = new ChangePasswordCommandHandler(_unitOfWork, _passwordService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task Handle_ShouldChangePassword_WhenCurrentPasswordIsValid()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, UserName = "user", PasswordHash = "oldHash" };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordService.VerifyPassword("OldPassword123!", "oldHash").Returns(true);
        _passwordService.CreatePasswordHash("NewPassword123!").Returns("newHash");

        var command = new ChangePasswordCommand("OldPassword123!", "NewPassword123!") { UserId = userId };
        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().BeOfType<SuccessResponse>();
        result.Success.Should().BeTrue();
        user.PasswordHash.Should().Be("newHash");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenCurrentPasswordIsInvalid()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, UserName = "user", PasswordHash = "oldHash" };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordService.VerifyPassword("WrongPassword123!", "oldHash").Returns(false);

        var command = new ChangePasswordCommand("WrongPassword123!", "NewPassword123!") { UserId = userId };
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenUserHasNoPassword()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, UserName = "cezuser", PasswordHash = null };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new ChangePasswordCommand("OldPassword123!", "NewPassword123!") { UserId = userId };
        Func<Task> act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
