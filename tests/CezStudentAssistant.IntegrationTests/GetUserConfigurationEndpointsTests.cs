using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.User;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CezStudentAssistant.IntegrationTests;

[TestFixture]
public class GetUserConfigurationEndpointsTests
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

    private async Task<Guid> GetCurrentUserIdFromDb(string username)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
        var user = await db.Users.FirstAsync(u => u.UserName == username);
        return user.Id;
    }

    [Test]
    public async Task GetUserConfiguration_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        var unauthorizedClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        var response = await unauthorizedClient.GetAsync("/user/configuration");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetUserConfiguration_ShouldReturnSuccessWithDefaults_WhenUserHasNoConfig()
    {
        // Arrange
        var username = "configuser1";
        await RegisterAndLogin(username, "Password123!");

        // Act
        var response = await _client.GetAsync("/user/configuration");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserConfigurationDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Be(UserMessageConsts.GetUserConfigurationSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.IsCezConnected.Should().BeFalse();
        result.Data.Theme.Should().Be(UserTheme.Light);
        result.Data.Language.Should().Be(UserLanguage.Polish);
    }

    [Test]
    public async Task GetUserConfiguration_ShouldReturnCustomConfig_WhenUserHasConfigAndCezUser()
    {
        // Arrange
        var username = "configuser2";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var cezUser = new CezUser
            {
                UserId = userId,
                FullName = "Jan Kowalski",
                Token = "fake-token",
                PrivateToken = "fake-ptoken",
                ExternalUserId = 12345
            };

            var config = new UserConfiguration
            {
                UserId = userId,
                Theme = UserTheme.Dark,
                Language = UserLanguage.English
            };

            db.CezUsers.Add(cezUser);
            db.UserConfigurations.Add(config);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/user/configuration");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserConfigurationDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsCezConnected.Should().BeTrue();
        result.Data.Theme.Should().Be(UserTheme.Dark);
        result.Data.Language.Should().Be(UserLanguage.English);
    }
}
