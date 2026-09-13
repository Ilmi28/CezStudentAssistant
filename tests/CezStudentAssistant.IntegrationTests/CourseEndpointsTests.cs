using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Dtos.Common;
using CezStudentAssistant.Application.Dtos.Course;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CezStudentAssistant.IntegrationTests;

public class CourseEndpointsTests
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
            AllowAutoRedirect = false,
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

    private async Task RegisterAndLogin(string username, string password)
    {
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", new RegisterUserCommand(username, password));
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginUserCommand(username, password));
        loginResponse.EnsureSuccessStatusCode();
    }

    private async Task<Guid> GetCurrentUserIdFromDb(string username)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
        var user = await db.Users.FirstAsync(u => u.UserName == username);
        return user.Id;
    }

    [Test]
    public async Task GetCourses_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        // Arrange
        var unauthorizedClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        // Act
        var response = await unauthorizedClient.GetAsync("/course");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetCourses_ShouldReturnCourses_WhenUserHasCourses()
    {
        // Arrange
        var username = "courseuser";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        // Seed some courses for the user
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var course1 = new Course { Name = "Test Course 1", CezExternalId = 101, Type = CourseType.Cez };
            var course2 = new Course { Name = "Test Course 2", CezExternalId = 102, Type = CourseType.Cez };
            
            db.Courses.AddRange(course1, course2);
            user.Courses.Add(course1);
            user.Courses.Add(course2);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/course");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResultDto<CourseDto>>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.Items.Should().HaveCount(2);
        
        var courseNames = content.Data.Items.Select(c => c.Name).ToList();
        courseNames.Should().Contain("Test Course 1");
        courseNames.Should().Contain("Test Course 2");
    }

    [Test]
    public async Task GetCourses_ShouldReturnEmptyList_WhenUserHasNoCourses()
    {
        // Arrange
        var username = "nocourseuser";
        await RegisterAndLogin(username, "Password123!");

        // Act
        var response = await _client.GetAsync("/course");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResultDto<CourseDto>>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.Items.Should().BeEmpty();
    }
}
