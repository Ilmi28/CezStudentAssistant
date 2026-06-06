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
    public async Task GetSiteInfo_ShouldReturnSiteInfo_WhenSuccessful()
    {
        // Arrange
        var request = new CezBaseRequest { Token = "token" };
        var externalResponse = new ExternalGetSiteInfoResponse { UserId = 123, FullName = "Site" };
        var requestResult = new CezRequestResult<ExternalGetSiteInfoResponse>(externalResponse, null);

        _requestService.SendGetAsync<ExternalGetSiteInfoResponse>(Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, string>>>())
            .Returns(requestResult);

        // Act
        var result = await _sut.GetSiteInfo(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data!.ExternalUserId.Should().Be(123);
    }
}
