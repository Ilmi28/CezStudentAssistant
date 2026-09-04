using CezStudentAssistant.Application.Commands.Auth;
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
            HandleCookies = true
        });
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
        var command = new RegisterUserCommand("testuser", "Password123!");

        var response = await _client.PostAsJsonAsync("/auth/register", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeTrue();
    }

    [Test]
    public async Task Register_ShouldReturnBadRequest_WhenValidationFails()
    {
        var command = new RegisterUserCommand("ab", "short");

        var response = await _client.PostAsJsonAsync("/auth/register", command);

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
        var command = new RegisterUserCommand("testuser", "Password123!");

        await _client.PostAsJsonAsync("/auth/register", command);
        var response = await _client.PostAsJsonAsync("/auth/register", command);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeFalse();
    }

    [Test]
    public async Task Login_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        var registerCommand = new RegisterUserCommand("loginuser", "Password123!");
        await _client.PostAsJsonAsync("/auth/register", registerCommand);

        var command = new LoginUserCommand("loginuser", "Password123!");

        var response = await _client.PostAsJsonAsync("/auth/login", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeTrue();

        response.Headers.Contains("Set-Cookie").Should().BeTrue();
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var registerCommand = new RegisterUserCommand("loginuser", "Password123!");
        await _client.PostAsJsonAsync("/auth/register", registerCommand);

        var command = new LoginUserCommand("loginuser", "WrongPassword123!");

        var response = await _client.PostAsJsonAsync("/auth/login", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeFalse();
    }

    [Test]
    public async Task LoginCez_ShouldCreateNewUser_WhenUserDoesNotExist()
    {
        var command = new LoginWithCezCommand("newcezuser", "password");
        SetupCezMock("newcezuser", "New CEZ User", 123);

        var response = await _client.PostAsJsonAsync("/auth/login-cez", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeTrue();
        response.Headers.Contains("Set-Cookie").Should().BeTrue();

        await AssertUserCoursesAsync("newcezuser", CustomWebApplicationFactory.DefaultCezCourses);
    }

    [Test]
    public async Task LoginCez_ShouldLinkToExistingUser_WhenUserExistsByUsername()
    {
        var username = "existinguser";
        await SeedUser(username);
        var command = new LoginWithCezCommand(username, "password");
        SetupCezMock(username, "Existing User Full Name", 456);

        var response = await _client.PostAsJsonAsync("/auth/login-cez", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeTrue();

        await AssertUserCoursesAsync(username, CustomWebApplicationFactory.DefaultCezCourses);
    }

    [Test]
    public async Task LoginCez_ShouldUpdateCezUser_WhenCezUserAlreadyLinked()
    {
        var username = "returningcezuser";
        var command = new LoginWithCezCommand(username, "password");
        SetupCezMock(username, "Returning User", 789, "token1", "ptoken1");
        await _client.PostAsJsonAsync("/auth/login-cez", command);

        SetupCezMock(username, "Returning User", 789, "token2", "ptoken2");

        var response = await _client.PostAsJsonAsync("/auth/login-cez", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeTrue();

        await AssertUserCoursesAsync(username, CustomWebApplicationFactory.DefaultCezCourses);
    }

    [Test]
    public async Task LoginCez_ShouldSyncCourses_WhenLoginSucceeds()
    {
        // Arrange
        var username = "synccourseuser";
        var command = new LoginWithCezCommand(username, "password");

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
        var response = await _client.PostAsJsonAsync("/auth/login-cez", command);

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
        var command = new LoginWithCezCommand("cezuser", "cezpassword");

        _factory.CezApiClientMock.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse { Success = false, Message = "Invalid credentials" });

        var response = await _client.PostAsJsonAsync("/auth/login-cez", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeFalse();
    }

    private void SetupCezMock(
        string username,
        string fullName,
        int externalId,
        string token = "t",
        string ptoken = "pt",
        IReadOnlyList<CezCourse>? courses = null)
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
                Data = (courses ?? CustomWebApplicationFactory.DefaultCezCourses).ToList()
            });
    }

    private async Task AssertUserCoursesAsync(string username, IReadOnlyList<CezCourse> expectedCourses)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();

        var user = await db.Users
            .Include(u => u.Courses)
            .FirstOrDefaultAsync(u => u.UserName == username);

        user.Should().NotBeNull();
        user!.Courses.Should().HaveCount(expectedCourses.Count);
        foreach (var course in expectedCourses)
        {
            user.Courses.Should().Contain(c => c.CezExternalId == course.ExternalId && c.Name == course.DisplayName);
        }
    }

    private async Task SeedUser(string username)
    {
        var command = new RegisterUserCommand(username, "Password123!");
        await _client.PostAsJsonAsync("/auth/register", command);
    }
}
