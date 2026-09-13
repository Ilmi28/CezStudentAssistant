#pragma warning disable NUnit1032
using CezStudentAssistant.Application.Commands.UserProfile;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using UserEntity = CezStudentAssistant.Domain.Entities.User;

namespace CezStudentAssistant.UnitTests.Application.Commands.UserProfile;

[TestFixture]
public class UpdateUserProfileCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IUserRepository _userRepository = null!;
    private UpdateUserProfileCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _handler = new UpdateUserProfileCommandHandler(_unitOfWork);
    }

    [Test]
    public async Task Handle_WhenValidProfileData_UpdatesUserAndReturnsProfile()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            UserName = "olduser",
            FullName = "Old Name",
            Email = "old@example.com"
        };

        var command = new UpdateUserProfileCommand
        {
            UserId = userId,
            UserName = "newuser",
            FullName = "New Name",
            Email = "new@example.com"
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), false, Arg.Any<System.Linq.Expressions.Expression<Func<UserEntity, object>>[]>())
            .Returns(user);

        _userRepository.GetSingleAsync(Arg.Any<System.Linq.Expressions.Expression<Func<UserEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        user.UserName.Should().Be("newuser");
        user.FullName.Should().Be("New Name");
        user.Email.Should().Be("new@example.com");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenUsernameTakenByAnotherUser_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "user1" };
        var otherUser = new UserEntity { Id = otherUserId, UserName = "takenuser" };

        var command = new UpdateUserProfileCommand
        {
            UserId = userId,
            UserName = "takenuser",
            FullName = "Test"
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), false, Arg.Any<System.Linq.Expressions.Expression<Func<UserEntity, object>>[]>())
            .Returns(user);

        _userRepository.GetSingleAsync(Arg.Any<System.Linq.Expressions.Expression<Func<UserEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(otherUser);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(UserMessageConsts.UsernameAlreadyTaken);
    }
}
