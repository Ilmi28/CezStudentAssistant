using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Commands.Course;
using CezStudentAssistant.Application.Dtos.Course;
using CezStudentAssistant.Application.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http.Headers;

namespace CezStudentAssistant.IntegrationTests;

public class CourseDetailsEndpointsTests
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
    public async Task GetCourseDetails_ShouldReturnDetailsAndEmptyFiles_WhenNoFilesUploaded()
    {
        await RegisterAndLogin("detailsuser", "Password123!");

        var addResponse = await _client.PostAsJsonAsync("/course", new AddUserCourseCommand { Name = "Details Course", Description = "Desc" });
        var addContent = await addResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>(_jsonOptions);
        var courseId = addContent!.Data;

        var response = await _client.GetAsync($"/course/{courseId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<CourseDetailsDto>>(_jsonOptions);
        
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.Name.Should().Be("Details Course");
        content.Data.Description.Should().Be("Desc");
        content.Data.Files.Should().BeEmpty();
    }

    [Test]
    public async Task UploadAndDownloadFile_ShouldSucceed_WhenValidRequest()
    {
        await RegisterAndLogin("fileuser", "Password123!");

        // Create course
        var addResponse = await _client.PostAsJsonAsync("/course", new AddUserCourseCommand { Name = "File Course" });
        var addContent = await addResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>(_jsonOptions);
        var courseId = addContent!.Data;

        // Upload file
        using var formData = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
        formData.Add(fileContent, "file", "testfile.bin");

        var uploadResponse = await _client.PostAsync($"/course/file/upload?courseId={courseId}", formData);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var uploadContent = await uploadResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>(_jsonOptions);
        var fileId = uploadContent!.Data;

        // Get Details
        var detailsResponse = await _client.GetAsync($"/course/{courseId}");
        var detailsContent = await detailsResponse.Content.ReadFromJsonAsync<ApiResponse<CourseDetailsDto>>(_jsonOptions);
        
        detailsContent!.Data!.Files.Should().HaveCount(1);
        detailsContent.Data.Files[0].DisplayName.Should().Be("testfile.bin");
        detailsContent.Data.Files[0].DownloadUrl.Should().Be($"/course/{courseId}/file/{fileId}/download");

        // Download File
        var downloadResponse = await _client.GetAsync($"/course/{courseId}/file/{fileId}/download");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        downloadResponse.Content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
        
        var downloadedBytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        downloadedBytes.Should().Equal(new byte[] { 1, 2, 3, 4, 5 });
    }
}
