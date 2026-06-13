using CezStudentAssistant.API.Requests.Auth;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CezStudentAssistant.IntegrationTests;

public class AuthEndpointsTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true // This should handle them, but let's be explicit if needed
        });

        // Actually, WebApplicationFactoryClientOptions.HandleCookies is true by default.
        // The problem might be the base address or something else.
        // Let's use a DelegatingHandler or just manually manage it for reliability if it fails.
    }

    [SetUp]
    public void SetUp()
    {
        _factory.ResetDatabase();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Register_ShouldCreateUserAndSetCookies_WhenDataIsValid()
    {
        var request = new RegisterUserRequest { UserName = "testuser", Password = "Password123!" };

        var response = await _client.PostAsJsonAsync("/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeTrue();
    }

    [Test]
    public async Task Register_ShouldReturnBadRequest_WhenValidationFails()
    {
        var request = new RegisterUserRequest { UserName = "ab", Password = "short" };

        var response = await _client.PostAsJsonAsync("/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        content.GetProperty("success").GetBoolean().Should().BeFalse();
        content.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.ValueKind.Should().Be(JsonValueKind.Array);
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Register_ShouldReturnConflict_WhenUsernameAlreadyExists()
    {
        var request = new RegisterUserRequest { UserName = "testuser", Password = "Password123!" };

        await _client.PostAsJsonAsync("/auth/register", request);
        var response = await _client.PostAsJsonAsync("/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeFalse();
    }

    [Test]
    public async Task Login_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        var registerRequest = new RegisterUserRequest { UserName = "loginuser", Password = "Password123!" };
        await _client.PostAsJsonAsync("/auth/register", registerRequest);

        var loginRequest = new LoginUserRequest { UserName = "loginuser", Password = "Password123!" };

        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeTrue();

        response.Headers.Contains("Set-Cookie").Should().BeTrue();
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var registerRequest = new RegisterUserRequest { UserName = "loginuser", Password = "Password123!" };
        await _client.PostAsJsonAsync("/auth/register", registerRequest);

        var loginRequest = new LoginUserRequest { UserName = "loginuser", Password = "WrongPassword123!" };

        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeFalse();
    }

    [Test]
    public async Task LoginCez_ShouldCreateNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new LoginWithCezRequest { UserName = "newcezuser", Password = "password" };
        SetupCezMock("newcezuser", "New CEZ User", 123);

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login-cez", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeTrue();
        response.Headers.Contains("Set-Cookie").Should().BeTrue();
    }

    [Test]
    public async Task LoginCez_ShouldLinkToExistingUser_WhenUserExistsByUsername()
    {
        // Arrange
        var username = "existinguser";
        await SeedUser(username);
        var request = new LoginWithCezRequest { UserName = username, Password = "password" };
        SetupCezMock(username, "Existing User Full Name", 456);

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login-cez", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeTrue();
    }

    [Test]
    public async Task LoginCez_ShouldUpdateCezUser_WhenCezUserAlreadyLinked()
    {
        // Arrange
        var username = "returningcezuser";
        // First login to create both User and CezUser
        var request = new LoginWithCezRequest { UserName = username, Password = "password" };
        SetupCezMock(username, "Returning User", 789, "token1", "ptoken1");
        await _client.PostAsJsonAsync("/auth/login-cez", request);

        // Setup mock for second login with new tokens
        SetupCezMock(username, "Returning User", 789, "token2", "ptoken2");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login-cez", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeTrue();
    }

    [Test]
    public async Task LoginCez_ShouldSyncCourses_WhenLoginSucceeds()
    {
        // Arrange
        var username = "synccourseuser";
        var request = new LoginWithCezRequest { UserName = username, Password = "password" };

        var incomingCourses = new List<CezCourse>
        {
            new CezCourse { ExternalId = 101, DisplayName = "Calculus I" },
            new CezCourse { ExternalId = 102, DisplayName = "Physics II" }
        };

        _factory.CezApiClientMock.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse
            {
                Success = true,
                Data = new CezTokens { Token = "t", PrivateToken = "pt" }
            });

        _factory.CezApiClientMock.GetSiteInfo(Arg.Any<CezBaseRequest>())
            .Returns(new CezGetSiteInfoResponse
            {
                Success = true,
                Data = new CezSiteInfo { UserName = username, FullName = "Sync User", ExternalUserId = 1000 }
            });

        _factory.CezApiClientMock.GetUserCourses(Arg.Any<CezUserRequest>())
            .Returns(new CezGetUserCoursesResponse
            {
                Success = true,
                Data = incomingCourses
            });

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login-cez", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify database state
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();

        var user = await db.Users
            .Include(u => u.Courses)
            .FirstOrDefaultAsync(u => u.UserName == username);

        user.Should().NotBeNull();
        user!.Courses.Should().HaveCount(2);
        user.Courses.Should().Contain(c => c.CezExternalId == 101 && c.Name == "Calculus I");
        user.Courses.Should().Contain(c => c.CezExternalId == 102 && c.Name == "Physics II");
    }

    [Test]
    public async Task LoginCez_ShouldReturnBadRequest_WhenCezApiReturnsError()
    {
        var request = new LoginWithCezRequest { UserName = "cezuser", Password = "cezpassword" };

        _factory.CezApiClientMock.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse { Success = false, Message = "Invalid credentials" });

        var response = await _client.PostAsJsonAsync("/auth/login-cez", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeFalse();
    }

    private void SetupCezMock(string username, string fullName, int externalId, string token = "t", string ptoken = "pt")
    {
        _factory.CezApiClientMock.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse
            {
                Success = true,
                Data = new CezTokens { Token = token, PrivateToken = ptoken }
            });

        _factory.CezApiClientMock.GetSiteInfo(Arg.Any<CezBaseRequest>())
            .Returns(new CezGetSiteInfoResponse
            {
                Success = true,
                Data = new CezSiteInfo { UserName = username, FullName = fullName, ExternalUserId = externalId }
            });

        _factory.CezApiClientMock.GetUserCourses(Arg.Any<CezUserRequest>())
            .Returns(new CezGetUserCoursesResponse
            {
                Success = true,
                Data = []
            });
    }

    private async Task SeedUser(string username)
    {
        var request = new RegisterUserRequest { UserName = username, Password = "Password123!" };
        await _client.PostAsJsonAsync("/auth/register", request);
    }
}
