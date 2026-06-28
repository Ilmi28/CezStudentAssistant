using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Application.Enums;
using System.IO;
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

    [Test]
    public async Task SyncCourses_ShouldSyncCourses_WhenUserExists()
    {
        // Arrange
        var username = "testuser";
        var password = "Password123!";
        await RegisterAndLogin(username, password);
        
        var userId = await GetCurrentUserIdFromDb(username);
        await LinkCezUser(userId, 12345);

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
        var response = await _client.PostAsync("/cez/sync-courses", null);

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
    public async Task SyncCourses_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        // Arrange - use a fresh client without cookies
        var unauthorizedClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        // Act
        var response = await unauthorizedClient.PostAsync("/cez/sync-courses", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task SyncCourses_ShouldReturnBadRequest_WhenCezApiReturnsError()
    {
        // Arrange
        var username = "erroruser";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);
        await LinkCezUser(userId, 54321);

        _factory.CezApiClientMock.GetUserCourses(Arg.Any<CezUserRequest>())
            .Returns(new CezGetUserCoursesResponse
            {
                Success = false,
                Message = "External API Error"
            });

        // Act
        var response = await _client.PostAsync("/cez/sync-courses", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("External API Error");
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

    private async Task LinkCezUser(Guid userId, long externalUserId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();

        var cezUser = new CezUser
        {
            UserId = userId,
            Token = "test-token",
            PrivateToken = "test-private-token",
            ExternalUserId = externalUserId,
            FullName = "Test User"
        };

        await db.CezUsers.AddAsync(cezUser);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task SyncCourses_ShouldSyncCourseContent_WhenContentsExist()
    {
        // Arrange
        var username = "contentsyncuser";
        var password = "Password123!";
        await RegisterAndLogin(username, password);
        
        var userId = await GetCurrentUserIdFromDb(username);
        await LinkCezUser(userId, 12345);

        var incomingCourses = new List<CezCourse>
        {
            new CezCourse { ExternalId = 301, DisplayName = "Advanced Database Systems" }
        };

        _factory.CezApiClientMock.GetUserCourses(Arg.Is<CezUserRequest>(r => r.UserId == 12345))
            .Returns(new CezGetUserCoursesResponse
            {
                Success = true,
                Data = incomingCourses
            });

        var timeCreated = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var timeModified = new DateTime(2026, 1, 1, 13, 0, 0, DateTimeKind.Utc);
        var courseContents = new List<CezCourseContent>
        {
            new CezCourseContent
            {
                FileName = "syllabus.pdf",
                Type = CezResourceType.File,
                MimeType = "application/pdf",
                FileUrl = "https://cez.test/files/syllabus.pdf",
                ModuleId = 999,
                TimeCreated = timeCreated,
                TimeModified = timeModified
            }
        };

        _factory.CezApiClientMock.GetCourseContent(Arg.Is<CezCourseRequest>(r => r.CourseId == 301))
            .Returns(new CezCourseContentResponse
            {
                Success = true,
                Data = courseContents
            });

        var fileStream = new MemoryStream(new byte[] { 4, 5, 6 });
        _factory.CezApiClientMock.DownloadCezFile(Arg.Is<CezFileRequest>(r => r.FileUrl == "https://cez.test/files/syllabus.pdf"))
            .Returns(fileStream);

        // Act
        var response = await _client.PostAsync("/cez/sync-courses", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the database has the course
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();

        var course = await db.Courses
            .FirstOrDefaultAsync(c => c.CezExternalId == 301);
        course.Should().NotBeNull();

        // Verify the database has the CezResource
        var resource = await db.Set<CezResource>()
            .FirstOrDefaultAsync(r => r.CourseId == course!.Id);
        resource.Should().NotBeNull();
        resource!.DisplayName.Should().Be("syllabus.pdf");
        resource.MimeType.Should().Be("application/pdf");
        resource.CezLastModified.Should().Be(timeModified);

        // Verify file actually exists in the Azurite blob storage
        var blobServiceClient = scope.ServiceProvider.GetRequiredService<Azure.Storage.Blobs.BlobServiceClient>();
        var containerClient = blobServiceClient.GetBlobContainerClient("course-files");
        var blobs = new List<string>();
        await foreach (var blob in containerClient.GetBlobsAsync())
        {
            blobs.Add(blob.Name);
        }
        blobs.Should().Contain(name => name.StartsWith($"{course!.Id}/999_") && name.EndsWith(".pdf"));

        // Download and verify content of the uploaded blob
        var resourceName = blobs.First(name => name.StartsWith($"{course!.Id}/999_") && name.EndsWith(".pdf"));
        var blobClient = containerClient.GetBlobClient(resourceName);
        using var downloadStream = new MemoryStream();
        await blobClient.DownloadToAsync(downloadStream);
        var uploadedBytes = downloadStream.ToArray();
        uploadedBytes.Should().Equal(new byte[] { 4, 5, 6 });
    }
}
