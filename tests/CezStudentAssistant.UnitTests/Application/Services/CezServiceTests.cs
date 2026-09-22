using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Application.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MockQueryable.NSubstitute;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Services;

[TestFixture]
public class CezServiceTests
{
    private ICezApiClient _cezApiClient = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IFileService _fileService = null!;
    private IAIClient _aiClient = null!;
    private IJobScheduler _jobScheduler = null!;
    private IJobService _jobService = null!;

    private IUserRepository _userRepository = null!;
    private ICezUserRepository _cezUserRepository = null!;
    private ICourseRepository _courseRepository = null!;
    private ICezResourceRepository _cezResourceRepository = null!;
    private IJobRepository _jobRepository = null!;

    private CezService _sut = null!;
    private Job _existingJob = null!;

    [SetUp]
    public void SetUp()
    {
        _cezApiClient = Substitute.For<ICezApiClient>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _fileService = Substitute.For<IFileService>();
        _aiClient = Substitute.For<IAIClient>();
        _jobScheduler = Substitute.For<IJobScheduler>();
        _userRepository = Substitute.For<IUserRepository>();
        _cezUserRepository = Substitute.For<ICezUserRepository>();
        _courseRepository = Substitute.For<ICourseRepository>();
        _cezResourceRepository = Substitute.For<ICezResourceRepository>();
        _jobRepository = Substitute.For<IJobRepository>();
        _jobService = Substitute.For<IJobService>();

        _unitOfWork.Repository<IUserRepository>().Returns(_userRepository);
        _unitOfWork.Repository<ICezUserRepository>().Returns(_cezUserRepository);
        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_cezResourceRepository);
        _unitOfWork.Repository<IJobRepository>().Returns(_jobRepository);

        _existingJob = new Job { JobId = "test-job-id", UserId = Guid.NewGuid(), Status = Domain.Enums.JobStatus.Enqueued };
        _jobService.GetLatestJobAsync(Arg.Any<Guid>(), JobType.CezSync, Arg.Any<CancellationToken>())
            .Returns(_existingJob);

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "BlobContainerSettings:CourseFilesContainer", "course-files" },
            { "CezSync:StaleSyncDaysThreshold", "1" }
        }).Build();
        _sut = new CezService(_cezApiClient, _unitOfWork, _fileService, _aiClient, _jobScheduler, _jobService, configuration);
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

        _cezApiClient.GetUser(Arg.Is<CezGetUserRequest>(r => r.Token == tokens.Token))
            .Returns(new CezGetUserResponse { Success = true, Data = siteInfo });
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

        _cezApiClient.GetUser(Arg.Any<CezGetUserRequest>())
            .Returns(new CezGetUserResponse { Success = false, Message = "Bad response" });

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

        await _jobService.Received(1).UpdateJobAsync(_existingJob, Domain.Enums.JobStatus.Processing, null, Arg.Any<CancellationToken>());
        await _jobService.Received(1).UpdateJobAsync(_existingJob, Domain.Enums.JobStatus.Failed, null, Arg.Any<CancellationToken>());
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

        await _jobService.Received(1).UpdateJobAsync(_existingJob, Domain.Enums.JobStatus.Processing, null, Arg.Any<CancellationToken>());
        await _jobService.Received(1).UpdateJobAsync(_existingJob, Domain.Enums.JobStatus.Failed, null, Arg.Any<CancellationToken>());
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

        var mockCourseDbSet = new List<Course> { existingCourse }.BuildMockDbSet();
        _courseRepository.Find(Arg.Any<Expression<Func<Course, bool>>>())
            .Returns(x => mockCourseDbSet);

        // Act
        await _sut.SyncUserCourses(userId, CancellationToken.None);

        // Assert
        // New Course
        await _courseRepository.Received(1).AddAsync(Arg.Is<Course>(c => c.CezExternalId == 1 && c.Name == "New Course"), Arg.Any<CancellationToken>());

        // Existing Course Update
        existingCourse.Name.Should().Be("Updated Course Name");
        user.Courses.Should().Contain(existingCourse);

        await _unitOfWork.Received().SaveChangesAsync(Arg.Any<CancellationToken>());

        await _jobService.Received(1).UpdateJobAsync(_existingJob, Domain.Enums.JobStatus.Processing, null, Arg.Any<CancellationToken>());
        await _jobService.Received(1).UpdateJobAsync(_existingJob, Domain.Enums.JobStatus.Succeeded, null, Arg.Any<CancellationToken>());
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

        var mockCourseDbSet = new List<Course> { existingCourse }.BuildMockDbSet();
        _courseRepository.Find(Arg.Any<Expression<Func<Course, bool>>>())
            .Returns(x => mockCourseDbSet);

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

        _cezResourceRepository.GetSingleAsync(Arg.Any<Expression<Func<Resource, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Resource)null!);

        var fileStream = new System.IO.MemoryStream(new byte[] { 1, 2, 3 });
        _cezApiClient.DownloadCezFile(Arg.Any<CezFileRequest>())
            .Returns(fileStream);

        // Act
        await _sut.SyncCourseContent(request, CancellationToken.None);

        // Assert
        await _cezResourceRepository.Received(1).AddAsync(Arg.Is<Resource>(r =>
            r.DisplayName == "file.pdf" &&
            r.MimeType == "application/pdf" &&
            r.CourseId == course.Id &&
            r.CezLastModified == timeModified), Arg.Any<CancellationToken>());

        await _fileService.Received(1).UploadAsync(
            Arg.Any<Stream>(),
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

        var existingResource = new Resource { Name = "123_1767272400.pdf", DisplayName = "file.pdf", MimeType = "application/pdf", CezLastModified = timeModified };
        _cezResourceRepository.GetSingleAsync(Arg.Any<Expression<Func<Resource, bool>>>(), Arg.Any<CancellationToken>())
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

        var existingResource = new Resource { Name = "123_1767272400.pdf", DisplayName = "file.pdf", MimeType = "application/pdf", CezLastModified = oldTimeModified, CourseId = course.Id };
        _cezResourceRepository.GetSingleAsync(Arg.Any<Expression<Func<Resource, bool>>>(), Arg.Any<CancellationToken>())
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
            Arg.Any<Stream>(),
            Arg.Is<string>(s => s.StartsWith($"{course.Id}/123_")),
            "course-files",
            "application/pdf",
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ConnectCezAsync_ShouldThrowConflictException_WhenUserAlreadyConnected()
    {
        var userId = Guid.NewGuid();
        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new CezUser { UserId = userId, Token = "token", PrivateToken = "pt", IsDisabled = false });

        Func<Task> act = () => _sut.ConnectCezAsync(userId, "username", "password", CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Test]
    public async Task ConnectCezAsync_ShouldThrowConflictException_WhenCezAccountLinkedToAnotherUser()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        _cezUserRepository.GetSingleAsync(Arg.Is<Expression<Func<CezUser, bool>>>(e => e.Compile().Invoke(new CezUser { UserId = userId, Token = "t", PrivateToken = "pt", IsDisabled = true })), Arg.Any<CancellationToken>())
            .Returns((CezUser?)null);

        SetupCezApiMocks("username", new CezTokens { Token = "t", PrivateToken = "pt" }, new CezSiteInfo { UserName = "username", FullName = "Full Name", ExternalUserId = 123 });

        _cezUserRepository.GetSingleAsync(Arg.Is<Expression<Func<CezUser, bool>>>(e => e.Compile().Invoke(new CezUser { ExternalUserId = 123, Token = "t", PrivateToken = "pt", IsDisabled = false })), Arg.Any<CancellationToken>())
            .Returns(new CezUser { UserId = otherUserId, ExternalUserId = 123, Token = "t", PrivateToken = "pt", IsDisabled = false });

        Func<Task> act = () => _sut.ConnectCezAsync(userId, "username", "password", CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Test]
    public async Task ConnectCezAsync_ShouldAddCezUser_WhenValid()
    {
        var userId = Guid.NewGuid();

        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((CezUser?)null);

        SetupCezApiMocks("username", new CezTokens { Token = "t", PrivateToken = "pt" }, new CezSiteInfo { UserName = "username", FullName = "Full Name", ExternalUserId = 123 });

        await _sut.ConnectCezAsync(userId, "username", "password", CancellationToken.None);

        await _cezUserRepository.Received(1).AddAsync(Arg.Is<CezUser>(cu => cu.UserId == userId && cu.ExternalUserId == 123 && !cu.IsDisabled), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ConnectCezAsync_ShouldEnableDisabledCezUser_WhenPreviouslyDisconnected()
    {
        var userId = Guid.NewGuid();
        var disabledCezUser = new CezUser
        {
            UserId = userId,
            ExternalUserId = 123,
            Token = "old_t",
            PrivateToken = "old_pt",
            IsDisabled = true
        };

        _cezUserRepository.GetSingleAsync(Arg.Any<Expression<Func<CezUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(disabledCezUser);

        SetupCezApiMocks("username", new CezTokens { Token = "t", PrivateToken = "pt" }, new CezSiteInfo { UserName = "username", FullName = "Full Name", ExternalUserId = 123 });

        await _sut.ConnectCezAsync(userId, "username", "password", CancellationToken.None);

        disabledCezUser.IsDisabled.Should().BeFalse();
        disabledCezUser.Token.Should().Be("t");
        disabledCezUser.PrivateToken.Should().Be("pt");

        await _cezUserRepository.DidNotReceive().AddAsync(Arg.Any<CezUser>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task DisconnectCezAsync_ShouldThrowBadRequestException_WhenUserNotConnected()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, UserName = "user", CezUser = null };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), Arg.Any<bool>(), Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        Func<Task> act = () => _sut.DisconnectCezAsync(userId, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Test]
    public async Task DisconnectCezAsync_ShouldThrowBadRequestException_WhenPureCezAccount()
    {
        var userId = Guid.NewGuid();
        var cezUser = new CezUser { UserId = userId, Token = "t", PrivateToken = "pt", IsDisabled = false };
        var user = new User { Id = userId, UserName = "cezuser", PasswordHash = null, CezUser = cezUser };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), Arg.Any<bool>(), Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        Func<Task> act = () => _sut.DisconnectCezAsync(userId, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Test]
    public async Task DisconnectCezAsync_ShouldSetIsDisabled_WhenValid()
    {
        var userId = Guid.NewGuid();
        var cezUser = new CezUser { UserId = userId, Token = "t", PrivateToken = "pt", IsDisabled = false };
        var user = new User { Id = userId, UserName = "user", PasswordHash = "hashedpassword", CezUser = cezUser };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>(), Arg.Any<bool>(), Arg.Any<Expression<Func<User, object>>[]>())
            .Returns(user);

        await _sut.DisconnectCezAsync(userId, CancellationToken.None);

        cezUser.IsDisabled.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SyncStaleCezCoursesAsync_ShouldThrowInvalidOperationException_WhenConfigMissing()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "BlobContainerSettings:CourseFilesContainer", "course-files" }
        }).Build();

        var sut = new CezService(_cezApiClient, _unitOfWork, _fileService, _aiClient, _jobScheduler, _jobService, configuration);

        Func<Task> act = () => sut.SyncStaleCezCoursesAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CezSync:StaleSyncDaysThreshold*");
    }

    [Test]
    public async Task SyncStaleCezCoursesAsync_ShouldDoNothing_WhenNoActiveCezUsers()
    {
        var emptyCezUsers = new List<CezUser>().BuildMockDbSet();
        _cezUserRepository.Find(Arg.Any<Expression<Func<CezUser, bool>>>())
            .Returns(emptyCezUsers);

        await _sut.SyncStaleCezCoursesAsync(CancellationToken.None);

        await _jobService.DidNotReceive().CreateJobAsync(Arg.Any<Guid>(), Arg.Any<JobType>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SyncStaleCezCoursesAsync_ShouldEnqueueSync_WhenUserHasNoPreviousJobOrJobIsStale()
    {
        var staleUserId = Guid.NewGuid();
        var freshUserId = Guid.NewGuid();

        var activeCezUsers = new List<CezUser>
        {
            new CezUser { UserId = staleUserId, Token = "t1", PrivateToken = "pt1", IsDisabled = false },
            new CezUser { UserId = freshUserId, Token = "t2", PrivateToken = "pt2", IsDisabled = false }
        }.BuildMockDbSet();

        _cezUserRepository.Find(Arg.Any<Expression<Func<CezUser, bool>>>())
            .Returns(activeCezUsers);

        var staleJob = new Job
        {
            UserId = staleUserId,
            JobId = "old-job",
            Status = JobStatus.Succeeded,
            Type = JobType.CezSync,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var freshJob = new Job
        {
            UserId = freshUserId,
            JobId = "fresh-job",
            Status = JobStatus.Succeeded,
            Type = JobType.CezSync,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        var allJobs = new List<Job> { staleJob, freshJob }.BuildMockDbSet();
        _jobRepository.Find(Arg.Any<Expression<Func<Job, bool>>>())
            .Returns(ci => allJobs.Where(ci.Arg<Expression<Func<Job, bool>>>()));

        var createdJob = new Job { UserId = staleUserId, JobId = "new-job", Status = JobStatus.Enqueued, Type = JobType.CezSync };
        _jobService.CreateJobAsync(staleUserId, JobType.CezSync, Arg.Any<CancellationToken>())
            .Returns(createdJob);

        _jobScheduler.Enqueue<ICezService>(Arg.Any<Expression<Action<ICezService>>>())
            .Returns("enqueued-job-id");

        await _sut.SyncStaleCezCoursesAsync(CancellationToken.None);

        await _jobService.Received(1).CreateJobAsync(staleUserId, JobType.CezSync, Arg.Any<CancellationToken>());
        await _jobService.DidNotReceive().CreateJobAsync(freshUserId, JobType.CezSync, Arg.Any<CancellationToken>());
        await _jobService.Received(1).UpdateJobAsync(createdJob, JobStatus.Enqueued, "enqueued-job-id", Arg.Any<CancellationToken>());
    }
}
