using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
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
    private ICezResourceRepository _cezResourceRepository = null!;
    private IFileService _fileService = null!;
    private IJobScheduler _jobScheduler = null!;
    private CezService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _cezApiClient = Substitute.For<ICezApiClient>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _fileService = Substitute.For<IFileService>();
        _userRepository = Substitute.For<IUserRepository>();
        _cezUserRepository = Substitute.For<ICezUserRepository>();
        _courseRepository = Substitute.For<ICourseRepository>();
        _cezResourceRepository = Substitute.For<ICezResourceRepository>();
        _jobScheduler = Substitute.For<IJobScheduler>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _unitOfWork.Repository<ICezUserRepository>().Returns(_cezUserRepository);
        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_cezResourceRepository);

        _sut = new CezService(_cezApiClient, _unitOfWork, _fileService, _jobScheduler);
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

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), includes: Arg.Any<Expression<Func<User, object>>[]>())
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

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), includes: Arg.Any<Expression<Func<User, object>>[]>())
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

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), includes: Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        _courseRepository.FindAsync(Arg.Any<Expression<Func<Course, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Course> { existingCourse });

        // Act
        await _sut.SyncUserCourses(userId, CancellationToken.None);

        // Assert
        user.Courses.Count.Should().Be(1);
    }

    [Test]
    public async Task SyncCourseContent_ShouldThrowBadRequestException_WhenCezApiFails()
    {
        // Arrange
        var request = new CezCourseRequest { CourseId = 1, Token = "token" };
        _cezApiClient.GetCourseContent(request)
            .Returns(new CezCourseContentResponse { Success = false, Message = "Failed" });

        // Act
        Func<Task> act = () => _sut.SyncCourseContent(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Failed");
    }

    [Test]
    public async Task SyncCourseContent_ShouldReturnEarly_WhenDataIsEmptyOrNull()
    {
        // Arrange
        var request = new CezCourseRequest { CourseId = 1, Token = "token" };
        _cezApiClient.GetCourseContent(request)
            .Returns(new CezCourseContentResponse { Success = true, Data = null });

        // Act
        await _sut.SyncCourseContent(request, CancellationToken.None);

        // Assert
        await _courseRepository.DidNotReceiveWithAnyArgs().GetSingleAsync(null!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Test]
    public async Task SyncCourseContent_ShouldThrowNotFoundException_WhenCourseDoesNotExist()
    {
        // Arrange
        var request = new CezCourseRequest { CourseId = 1, Token = "token" };
        var incomingContent = new List<CezCourseContent>
        {
            new CezCourseContent { FileName = "file.txt", Type = CezResourceType.File, MimeType = "text/plain", FileUrl = "url", ModuleId = 123 }
        };
        _cezApiClient.GetCourseContent(request)
            .Returns(new CezCourseContentResponse { Success = true, Data = incomingContent });

        _courseRepository.GetSingleAsync(Arg.Any<Expression<Func<Course, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Course)null!);

        // Act
        Func<Task> act = () => _sut.SyncCourseContent(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task SyncCourseContent_ShouldAddNewResourceAndUploadFile_WhenResourceDoesNotExist()
    {
        // Arrange
        var request = new CezCourseRequest { CourseId = 1, Token = "token" };
        var timeCreated = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var timeModified = new DateTime(2026, 1, 1, 13, 0, 0, DateTimeKind.Utc);
        var incomingContent = new List<CezCourseContent>
        {
            new CezCourseContent 
            { 
                FileName = "file.pdf", 
                Type = CezResourceType.File, 
                MimeType = "application/pdf", 
                FileUrl = "url", 
                ModuleId = 123,
                TimeCreated = timeCreated,
                TimeModified = timeModified
            }
        };
        _cezApiClient.GetCourseContent(request)
            .Returns(new CezCourseContentResponse { Success = true, Data = incomingContent });

        var course = new Course { Id = Guid.NewGuid(), Name = "Course", CezExternalId = 1 };
        _courseRepository.GetSingleAsync(Arg.Any<Expression<Func<Course, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(course);

        _cezResourceRepository.GetSingleAsync(Arg.Any<Expression<Func<CezResource, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((CezResource)null!);

        var fileStream = new System.IO.MemoryStream(new byte[] { 1, 2, 3 });
        _cezApiClient.DownloadCezFile(Arg.Any<CezFileRequest>())
            .Returns(fileStream);

        // Act
        await _sut.SyncCourseContent(request, CancellationToken.None);

        // Assert
        await _cezResourceRepository.Received(1).AddAsync(Arg.Is<CezResource>(r => 
            r.DisplayName == "file.pdf" && 
            r.MimeType == "application/pdf" && 
            r.CourseId == course.Id && 
            r.CezLastModified == timeModified), Arg.Any<CancellationToken>());

        await _fileService.Received(1).UploadAsync(
            fileStream, 
            Arg.Is<string>(s => s.StartsWith($"{course.Id}/123_")), 
            "course-files", 
            "application/pdf", 
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SyncCourseContent_ShouldNotUpdate_WhenResourceExistsWithSameModifiedTime()
    {
        // Arrange
        var request = new CezCourseRequest { CourseId = 1, Token = "token" };
        var timeCreated = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var timeModified = new DateTime(2026, 1, 1, 13, 0, 0, DateTimeKind.Utc);
        var incomingContent = new List<CezCourseContent>
        {
            new CezCourseContent 
            { 
                FileName = "file.pdf", 
                Type = CezResourceType.File, 
                MimeType = "application/pdf", 
                FileUrl = "url", 
                ModuleId = 123,
                TimeCreated = timeCreated,
                TimeModified = timeModified
            }
        };
        _cezApiClient.GetCourseContent(request)
            .Returns(new CezCourseContentResponse { Success = true, Data = incomingContent });

        var course = new Course { Id = Guid.NewGuid(), Name = "Course", CezExternalId = 1 };
        _courseRepository.GetSingleAsync(Arg.Any<Expression<Func<Course, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(course);

        var existingResource = new CezResource { Name = "123_1767272400.pdf", DisplayName = "file.pdf", MimeType = "application/pdf", CezLastModified = timeModified };
        _cezResourceRepository.GetSingleAsync(Arg.Any<Expression<Func<CezResource, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingResource);

        // Act
        await _sut.SyncCourseContent(request, CancellationToken.None);

        // Assert
        await _cezResourceRepository.DidNotReceiveWithAnyArgs().AddAsync(null!, default);
        await _fileService.DidNotReceiveWithAnyArgs().UploadAsync(null!, null!, null!, null!, default);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    [Test]
    public async Task SyncCourseContent_ShouldUpdateResourceAndUploadFile_WhenResourceExistsWithDifferentModifiedTime()
    {
        // Arrange
        var request = new CezCourseRequest { CourseId = 1, Token = "token" };
        var timeCreated = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var oldTimeModified = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var newTimeModified = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc);
        var incomingContent = new List<CezCourseContent>
        {
            new CezCourseContent 
            { 
                FileName = "file_updated.pdf", 
                Type = CezResourceType.File, 
                MimeType = "application/pdf", 
                FileUrl = "url", 
                ModuleId = 123,
                TimeCreated = timeCreated,
                TimeModified = newTimeModified
            }
        };
        _cezApiClient.GetCourseContent(request)
            .Returns(new CezCourseContentResponse { Success = true, Data = incomingContent });

        var course = new Course { Id = Guid.NewGuid(), Name = "Course", CezExternalId = 1 };
        _courseRepository.GetSingleAsync(Arg.Any<Expression<Func<Course, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(course);

        var existingResource = new CezResource { Name = "123_1767272400.pdf", DisplayName = "file.pdf", MimeType = "application/pdf", CezLastModified = oldTimeModified, CourseId = course.Id };
        _cezResourceRepository.GetSingleAsync(Arg.Any<Expression<Func<CezResource, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingResource);

        var fileStream = new System.IO.MemoryStream(new byte[] { 4, 5, 6 });
        _cezApiClient.DownloadCezFile(Arg.Any<CezFileRequest>())
            .Returns(fileStream);

        // Act
        await _sut.SyncCourseContent(request, CancellationToken.None);

        // Assert
        await _cezResourceRepository.DidNotReceiveWithAnyArgs().AddAsync(null!, default);
        
        existingResource.DisplayName.Should().Be("file_updated.pdf");
        existingResource.CezLastModified.Should().Be(newTimeModified);

        await _fileService.Received(1).UploadAsync(
            fileStream, 
            Arg.Is<string>(s => s.StartsWith($"{course.Id}/123_")), 
            "course-files", 
            "application/pdf", 
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
