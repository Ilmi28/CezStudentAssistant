using AutoMapper;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Cez;
using CezStudentAssistant.Cez.Interfaces;
using CezStudentAssistant.Cez.Responses;
using CezStudentAssistant.Cez.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CezStudentAssistant.UnitTests.Cez;

public class CezApiClientTests
{
    private ICezRequestService _requestService = null!;
    private IMapper _mapper = null!;
    private CezApiClient _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _requestService = Substitute.For<ICezRequestService>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<CezProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _sut = new CezApiClient(_requestService, _mapper);
    }

    [Test]
    public async Task LoginToCez_ShouldReturnSuccessResponse_WhenLoginSuccessful()
    {
        // Arrange
        var loginDto = new CezLoginRequest { UserName = "testuser", Password = "testpassword" };
        var externalResponse = new ExternalCezLoginResponse { Token = "token", PrivateToken = "private" };
        var requestResult = new CezRequestResult<ExternalCezLoginResponse>(externalResponse, null);

        _requestService.SendGetAsync<ExternalCezLoginResponse>(Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>())
            .Returns(requestResult);

        // Act
        var result = await _sut.LoginToCez(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be("token");
        result.Data!.PrivateToken.Should().Be("private");
    }

    [Test]
    public async Task LoginToCez_ShouldReturnErrorResponse_WhenLoginFails()
    {
        // Arrange
        var loginDto = new CezLoginRequest { UserName = "testuser", Password = "testpassword" };
        var errorResponse = new ExternalCezErrorResponse { ErrorCode = "invalidlogin", Message = "Invalid login" };
        var requestResult = new CezRequestResult<ExternalCezLoginResponse>(null, errorResponse);

        _requestService.SendGetAsync<ExternalCezLoginResponse>(Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>())
            .Returns(requestResult);

        // Act
        var result = await _sut.LoginToCez(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("invalidlogin");
    }

    [Test]
    public async Task GetUserCourses_ShouldReturnCourses_WhenSuccessful()
    {
        // Arrange
        var request = new CezUserRequest { Token = "token", UserId = 123 };
        var externalResponse = new List<ExternalCezGetUserCoursesResponse>
        {
            new() { Id = 1, FullName = "Course 1" }
        };
        var requestResult = new CezRequestResult<List<ExternalCezGetUserCoursesResponse>>(externalResponse, null);

        _requestService.SendGetAsync<List<ExternalCezGetUserCoursesResponse>>(Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>())
            .Returns(requestResult);

        // Act
        var result = await _sut.GetUserCourses(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data!.First().ExternalId.Should().Be(1);
    }

    [Test]
    public async Task GetUser_ShouldReturnUserInfo_WhenSuccessful()
    {
        // Arrange
        var request = new CezGetUserRequest { Token = "token", Field = "username", Value = "testuser" };
        var externalResponse = new List<ExternalCezGetUserResponse>
        {
            new() { UserId = 123, FullName = "Site", Email = "test@example.com" }
        };
        var requestResult = new CezRequestResult<List<ExternalCezGetUserResponse>>(externalResponse, null);

        _requestService.SendGetAsync<List<ExternalCezGetUserResponse>>(Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>())
            .Returns(requestResult);

        // Act
        var result = await _sut.GetUser(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data!.ExternalUserId.Should().Be(123);
        result.Data!.Email.Should().Be("test@example.com");
    }
    [Test]
    public async Task GetCourseContent_ShouldReturnCourseContent_WhenSuccessful()
    {
        // Arrange
        var request = new CezCourseRequest { Token = "token", CourseId = 401 };
        var externalResponse = new List<ExternalCezCourseSection>
        {
            new() { Id = 1 }
        };
        var requestResult = new CezRequestResult<List<ExternalCezCourseSection>>(externalResponse, null);

        _requestService.SendGetAsync<List<ExternalCezCourseSection>>(Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>())
            .Returns(requestResult);

        // Act
        var result = await _sut.GetCourseContent(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Test]
    public async Task DownloadCezFile_ShouldReturnStream_WhenSuccessful()
    {
        // Arrange
        var request = new CezFileRequest { Token = "token", FileUrl = "https://example.com/file" };
        var stream = new System.IO.MemoryStream();

        _requestService.DownloadFileAsync(request.FileUrl, request.Token)
            .Returns(stream);

        // Act
        var result = await _sut.DownloadCezFile(request);

        // Assert
        result.Should().BeSameAs(stream);
    }
}
