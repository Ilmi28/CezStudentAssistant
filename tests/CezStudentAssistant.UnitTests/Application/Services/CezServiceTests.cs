using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Application.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using System.Linq.Expressions;

namespace CezStudentAssistant.UnitTests.Application.Services;

public class CezServiceTests
{
    private ICezApiClient _cezApiClient = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IUserRepository _userRepository = null!;
    private ICezUserRepository _cezUserRepository = null!;
    private ICourseRepository _courseRepository = null!;
    private CezService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _cezApiClient = Substitute.For<ICezApiClient>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _userRepository = Substitute.For<IUserRepository>();
        _cezUserRepository = Substitute.For<ICezUserRepository>();
        _courseRepository = Substitute.For<ICourseRepository>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _unitOfWork.Repository<ICezUserRepository>().Returns(_cezUserRepository);
        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);

        _sut = new CezService(_cezApiClient, _unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    private void SetupCezApiMocks(string userName, CezTokens tokens, CezSiteInfo siteInfo)
    {
        _cezApiClient.LoginToCez(Arg.Is<CezLoginRequest>(r => r.UserName == userName))
            .Returns(new CezLoginResponse { Success = true, Data = tokens });

        _cezApiClient.GetSiteInfo(Arg.Is<CezBaseRequest>(r => r.Token == tokens.Token))
            .Returns(new CezGetSiteInfoResponse { Success = true, Data = siteInfo });
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldCreateNewUser_WhenUserDoesNotExist()
    {
        var userName = "cezUser";
        var password = "password";
        var tokens = new CezTokens { Token = "t", PrivateToken = "pt" };
        var siteInfo = new CezSiteInfo { UserName = userName, FullName = "Full Name", ExternalUserId = 123 };
        SetupCezApiMocks(userName, tokens, siteInfo);

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _sut.LoginWithCezAsync(userName, password, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
        await _cezUserRepository.Received(1).AddAsync(
            Arg.Is<CezUser>(cu =>
                cu.FullName == "Full Name" &&
                cu.Token == "t" &&
                cu.PrivateToken == "pt" &&
                cu.ExternalUserId == 123 &&
                cu.User.UserName == userName
            ),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldThrowBadRequest_WhenLoginFails()
    {
        var userName = "cezUser";
        var password = "password";

        _cezApiClient.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse { Success = false, Message = "Invalid credentials" });

        Func<Task> act = () => _sut.LoginWithCezAsync(userName, password, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldThrowBadRequest_WhenSiteInfoFails()
    {
        var userName = "cezUser";
        var password = "password";
        var tokens = new CezTokens { Token = "t", PrivateToken = "pt" };

        _cezApiClient.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse { Success = true, Data = tokens });

        _cezApiClient.GetSiteInfo(Arg.Any<CezBaseRequest>())
            .Returns(new CezGetSiteInfoResponse { Success = false, Message = "Bad response" });

        Func<Task> act = () => _sut.LoginWithCezAsync(userName, password, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldThrowBadRequest_WhenUserNameIsNullAndUserDoesNotExist()
    {
        var userName = "cezUser";
        var password = "password";
        var tokens = new CezTokens { Token = "t", PrivateToken = "pt" };
        var siteInfo = new CezSiteInfo { UserName = null!, FullName = "Full Name", ExternalUserId = 123 };
        SetupCezApiMocks(userName, tokens, siteInfo);

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        Func<Task> act = () => _sut.LoginWithCezAsync(userName, password, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldCreateNewCezUser_WhenCezUserIsMissing()
    {
        var userName = "cezUser";
        var password = "password";
        var userId = Guid.NewGuid();
        var user = new User { UserName = userName };
        user.GetType().GetProperty("Id")?.SetValue(user, userId);

        var tokens = new CezTokens { Token = "new_t", PrivateToken = "new_pt" };
        var siteInfo = new CezSiteInfo { UserName = userName, FullName = "New Full Name", ExternalUserId = 789 };
        SetupCezApiMocks(userName, tokens, siteInfo);

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((CezUser?)null);

        await _sut.LoginWithCezAsync(userName, password, CancellationToken.None);

        await _cezUserRepository.Received(1).AddAsync(
            Arg.Is<CezUser>(cu =>
                cu.UserId == userId &&
                cu.Token == "new_t" &&
                cu.PrivateToken == "new_pt" &&
                cu.FullName == "New Full Name" &&
                cu.ExternalUserId == 789
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldUpdateExistingUser_WhenUserExists()
    {
        var userName = "cezUser";
        var password = "password";
        var userId = Guid.NewGuid();
        var user = new User { UserName = userName };
        user.GetType().GetProperty("Id")?.SetValue(user, userId);

        var existingCezUser = new CezUser
        {
            UserId = userId,
            Token = "old",
            FullName = "Name",
            PrivateToken = "pt",
            ExternalUserId = 1,
            User = user
        };

        var tokens = new CezTokens { Token = "new_t", PrivateToken = "new_pt" };
        var siteInfo = new CezSiteInfo { UserName = userName, FullName = "New Name", ExternalUserId = 2 };
        SetupCezApiMocks(userName, tokens, siteInfo);

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingCezUser);

        var result = await _sut.LoginWithCezAsync(userName, password, CancellationToken.None);

        result.Should().Be(userId);
        existingCezUser.Token.Should().Be("new_t");
        existingCezUser.PrivateToken.Should().Be("new_pt");
        existingCezUser.FullName.Should().Be("New Name");
        existingCezUser.ExternalUserId.Should().Be(2);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task LoginWithCezAsync_ShouldHandleNullTokens_BySettingEmptyStrings()
    {
        var userName = "cezUser";
        var password = "password";
        var userId = Guid.NewGuid();
        var user = new User { UserName = userName };
        user.GetType().GetProperty("Id")?.SetValue(user, userId);

        var tokens = new CezTokens { Token = null!, PrivateToken = null! };
        var siteInfo = new CezSiteInfo { UserName = userName };
        SetupCezApiMocks(userName, tokens, siteInfo);

        _userRepository.GetSingleAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((CezUser?)null);

        await _sut.LoginWithCezAsync(userName, password, CancellationToken.None);

        await _cezUserRepository.Received(1).AddAsync(
            Arg.Is<CezUser>(cu =>
                cu.Token == string.Empty &&
                cu.PrivateToken == string.Empty
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Test]
    public async Task SyncUserCourses_ShouldThrowNotFound_WhenCezUserIsMissing()
    {
        var userId = Guid.NewGuid();
        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((CezUser?)null);

        Func<Task> act = () => _sut.SyncUserCourses(userId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task SyncUserCourses_ShouldThrowBadRequest_WhenApiFails()
    {
        var userId = Guid.NewGuid();
        var cezUser = new CezUser { UserId = userId, Token = "t", ExternalUserId = 1, PrivateToken = "pt" };
        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(cezUser);

        _cezApiClient.GetUserCourses(Arg.Any<CezUserRequest>())
            .Returns(new CezGetUserCoursesResponse { Success = false, Message = "Error" });

        Func<Task> act = () => _sut.SyncUserCourses(userId, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Test]
    public async Task SyncUserCourses_ShouldThrowNotFound_WhenUserIsMissing()
    {
        var userId = Guid.NewGuid();
        var cezUser = new CezUser { UserId = userId, Token = "t", ExternalUserId = 1, PrivateToken = "pt" };
        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(cezUser);

        _cezApiClient.GetUserCourses(Arg.Any<CezUserRequest>())
            .Returns(new CezGetUserCoursesResponse { Success = true, Data = new List<CezCourse>() });

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), Arg.Any<Expression<Func<User, object>>[]>())
            .Returns((User?)null);

        Func<Task> act = () => _sut.SyncUserCourses(userId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task SyncUserCourses_ShouldSyncNewAndExistingCourses_WhenDataIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cezUser = new CezUser { UserId = userId, Token = "token", ExternalUserId = 100, PrivateToken = "pt" };
        var user = new User { Id = userId, UserName = "user", Courses = new List<Course>() };

        var incomingCourses = new List<CezCourse>
        {
            new CezCourse { ExternalId = 1, DisplayName = "New Course" },
            new CezCourse { ExternalId = 2, DisplayName = "Updated Course Name" }
        };

        var existingCourse = new Course { CezExternalId = 2, Name = "Old Name" };

        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(cezUser);

        _cezApiClient.GetUserCourses(Arg.Any<CezUserRequest>())
            .Returns(new CezGetUserCoursesResponse { Success = true, Data = incomingCourses });

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        _courseRepository.FindAsync(Arg.Any<Expression<Func<Course, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Course> { existingCourse });

        // Act
        await _sut.SyncUserCourses(userId, CancellationToken.None);

        // Assert
        // New Course
        await _courseRepository.Received(1).AddAsync(Arg.Is<Course>(c => c.CezExternalId == 1 && c.Name == "New Course"), Arg.Any<CancellationToken>());

        // Existing Course Update
        existingCourse.Name.Should().Be("Updated Course Name");
        user.Courses.Should().Contain(existingCourse);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SyncUserCourses_ShouldNotDuplicateCourseInUserCollection_WhenAlreadyAssociated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cezUser = new CezUser { UserId = userId, Token = "token", ExternalUserId = 100, PrivateToken = "pt" };
        var existingCourse = new Course { CezExternalId = 2, Name = "Name" };
        var user = new User { Id = userId, UserName = "user", Courses = new List<Course> { existingCourse } };

        var incomingCourses = new List<CezCourse>
        {
            new CezCourse { ExternalId = 2, DisplayName = "Name" }
        };

        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(cezUser);

        _cezApiClient.GetUserCourses(Arg.Any<CezUserRequest>())
            .Returns(new CezGetUserCoursesResponse { Success = true, Data = incomingCourses });

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        _courseRepository.FindAsync(Arg.Any<Expression<Func<Course, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Course> { existingCourse });

        // Act
        await _sut.SyncUserCourses(userId, CancellationToken.None);

        // Assert
        user.Courses.Count.Should().Be(1);
    }
}
