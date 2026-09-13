using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.User;
using CezStudentAssistant.Application.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CezStudentAssistant.IntegrationTests;

[TestFixture]
public class DashboardEndpointsTests
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
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", new RegisterUserCommand(username, $"{username}@example.com", password));
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new LoginUserCommand(username, password));
        loginResponse.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task GetDashboardStats_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        var unauthorizedClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        var response = await unauthorizedClient.GetAsync("/user/dashboard-stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetDashboardStats_ShouldReturnSuccessAndStats_WhenLoggedIn()
    {
        // Arrange
        await RegisterAndLogin("dashuser1", "Password123!");

        // Act
        var response = await _client.GetAsync("/user/dashboard-stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardStatsDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Be(UserMessageConsts.GetDashboardStatsSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.CourseCount.Should().Be(0);
        result.Data.QuizCount.Should().Be(0);
        result.Data.FlashcardDeckCount.Should().Be(0);
        result.Data.FlashcardCount.Should().Be(0);
    }

    [Test]
    public async Task GetRecentActivity_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        var unauthorizedClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        var response = await unauthorizedClient.GetAsync("/user/recent-activity");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetRecentActivity_ShouldReturnSuccessAndEmptyList_WhenNoAttemptsExist()
    {
        // Arrange
        await RegisterAndLogin("dashuser2", "Password123!");

        // Act
        var response = await _client.GetAsync("/user/recent-activity");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<RecentActivityDto>>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Be(UserMessageConsts.GetRecentActivitySuccess);
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }
}
