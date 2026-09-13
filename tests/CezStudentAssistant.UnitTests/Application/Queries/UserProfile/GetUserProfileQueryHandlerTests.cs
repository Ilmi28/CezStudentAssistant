#pragma warning disable NUnit1032
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.UserProfile;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using UserEntity = CezStudentAssistant.Domain.Entities.User;

namespace CezStudentAssistant.UnitTests.Application.Queries.UserProfile;

[TestFixture]
public class GetUserProfileQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IUserRepository _userRepository = null!;
    private GetUserProfileQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _handler = new GetUserProfileQueryHandler(_unitOfWork);
    }

    [Test]
    public async Task Handle_WhenUserExists_ReturnsUserProfile()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserProfileQuery { UserId = userId };
        var user = new UserEntity
        {
            Id = userId,
            UserName = "testuser",
            FullName = "Jan Kowalski",
            Email = "jan@example.com",
            CezUser = new CezUser
            {
                UserId = userId,
                FullName = "Jan Kowalski CEZ",
                Email = "jan.cez@example.com",
                Token = "token",
                PrivateToken = "private"
            }
        };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), true, Arg.Any<System.Linq.Expressions.Expression<Func<UserEntity, object>>[]>())
            .Returns(user);

        var response = await _handler.Handle(query, CancellationToken.None);

        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.UserName.Should().Be("testuser");
        response.Data.FullName.Should().Be("Jan Kowalski");
        response.Data.Email.Should().Be("jan@example.com");
        response.Data.IsCezConnected.Should().BeTrue();
        response.Data.CezFullName.Should().Be("Jan Kowalski CEZ");
        response.Data.CezEmail.Should().Be("jan.cez@example.com");
    }

    [Test]
    public async Task Handle_WhenUserNotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserProfileQuery { UserId = userId };

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), true, Arg.Any<System.Linq.Expressions.Expression<Func<UserEntity, object>>[]>())
            .Returns((UserEntity?)null);

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
