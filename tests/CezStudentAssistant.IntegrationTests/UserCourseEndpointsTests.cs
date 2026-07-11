using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Commands.Course;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CezStudentAssistant.IntegrationTests;

public class UserCourseEndpointsTests
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
        await _client.PostAsJsonAsync("/auth/register", new RegisterUserCommand(username, password));
        await _client.PostAsJsonAsync("/auth/login", new LoginUserCommand(username, password));
    }

    [Test]
    public async Task CreateUserCourse_ShouldCreateCourse_WhenValidRequest()
    {
        await RegisterAndLogin("createcourseuser", "Password123!");

        var command = new AddUserCourseCommand { Name = "My Custom Course", Description = "A description" };
        var response = await _client.PostAsJsonAsync("/course", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(_jsonOptions);
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeEmpty();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
        var course = await db.Courses.Include(c => c.Users).FirstOrDefaultAsync(c => c.Id == content.Data);

        course.Should().NotBeNull();
        course!.Name.Should().Be("My Custom Course");
        course.Type.Should().Be(CourseType.User);
        course.Users.Should().Contain(u => u.UserName == "createcourseuser");
    }

    [Test]
    public async Task UpdateUserCourse_ShouldUpdateCourse_WhenValidRequest()
    {
        await RegisterAndLogin("updatecourseuser", "Password123!");

        var addResponse = await _client.PostAsJsonAsync("/course", new AddUserCourseCommand { Name = "Old Name" });
        var addContent = await addResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>(_jsonOptions);
        var courseId = addContent!.Data;

        var updateCommand = new UpdateUserCourseCommand { Name = "New Name", Description = "New Desc" };
        var response = await _client.PutAsJsonAsync($"/course/{courseId}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
        var course = await db.Courses.FindAsync(courseId);

        course!.Name.Should().Be("New Name");
        course.Description.Should().Be("New Desc");
    }

    [Test]
    public async Task DeleteUserCourse_ShouldDeleteCourse_WhenValidRequest()
    {
        await RegisterAndLogin("deletecourseuser", "Password123!");

        var addResponse = await _client.PostAsJsonAsync("/course", new AddUserCourseCommand { Name = "To Be Deleted" });
        var addContent = await addResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>(_jsonOptions);
        var courseId = addContent!.Data;

        var response = await _client.DeleteAsync($"/course/{courseId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
        var course = await db.Courses.FindAsync(courseId);

        course.Should().BeNull();
    }
}
