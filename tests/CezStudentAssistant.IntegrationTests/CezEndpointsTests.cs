using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CezStudentAssistant.IntegrationTests;

public class CezEndpointsTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
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
    public async Task SyncCourses_ShouldSyncCourses_WhenUserExists()
    {
        // Arrange
        var userId = await CreateUserAndCezUser("testuser", 12345);
        
        var incomingCourses = new List<CezCourse>
        {
            new CezCourse { ExternalId = 201, DisplayName = "Data Structures" },
            new CezCourse { ExternalId = 202, DisplayName = "Algorithms" }
        };

        _factory.CezApiClientMock.GetUserCourses(Arg.Is<CezUserRequest>(r => r.UserId == 12345))
            .Returns(new CezGetUserCoursesResponse
            {
                Success = true,
                Data = incomingCourses
            });

        // Act
        var response = await _client.PostAsync($"/cez/sync-courses?userId={userId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeTrue();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
        
        var user = await db.Users
            .Include(u => u.Courses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        user.Should().NotBeNull();
        user!.Courses.Should().HaveCount(2);
        user.Courses.Should().Contain(c => c.CezExternalId == 201 && c.Name == "Data Structures");
        user.Courses.Should().Contain(c => c.CezExternalId == 202 && c.Name == "Algorithms");
    }

    [Test]
    public async Task SyncCourses_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Act
        var response = await _client.PostAsync($"/cez/sync-courses?userId={Guid.NewGuid()}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SyncCourses_ShouldReturnBadRequest_WhenCezApiReturnsError()
    {
        // Arrange
        var userId = await CreateUserAndCezUser("erroruser", 54321);

        _factory.CezApiClientMock.GetUserCourses(Arg.Any<CezUserRequest>())
            .Returns(new CezGetUserCoursesResponse
            {
                Success = false,
                Message = "External API Error"
            });

        // Act
        var response = await _client.PostAsync($"/cez/sync-courses?userId={userId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("External API Error");
    }

    private async Task<Guid> CreateUserAndCezUser(string username, long externalUserId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();

        var user = new User { UserName = username };
        var cezUser = new CezUser
        {
            User = user,
            Token = "test-token",
            PrivateToken = "test-private-token",
            ExternalUserId = externalUserId,
            FullName = "Test User"
        };

        await db.Users.AddAsync(user);
        await db.CezUsers.AddAsync(cezUser);
        await db.SaveChangesAsync();

        return user.Id;
    }
}
