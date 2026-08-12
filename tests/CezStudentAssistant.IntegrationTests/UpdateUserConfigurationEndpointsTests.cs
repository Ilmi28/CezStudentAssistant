using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Commands.UserConfiguration;
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
public class UpdateUserConfigurationEndpointsTests
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
    public async Task UpdateUserConfiguration_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        var unauthorizedClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        var command = new UpdateUserConfigurationCommand
        {
            Theme = UserTheme.Dark,
            Language = UserLanguage.English
        };

        var response = await unauthorizedClient.PutAsJsonAsync("/user/configuration", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task UpdateUserConfiguration_ShouldCreateConfigWithDefaults_WhenUserHasNoConfigAndNullPayload()
    {
        // Arrange
        var username = "updateconfig_defaults";
        await RegisterAndLogin(username, "Password123!");

        var command = new UpdateUserConfigurationCommand
        {
            Theme = null,
            Language = null
        };

        // Act
        var response = await _client.PutAsJsonAsync("/user/configuration", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserConfigurationDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Be(UserMessageConsts.UpdateUserConfigurationSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Theme.Should().Be(UserTheme.Light);
        result.Data.Language.Should().Be(UserLanguage.Polish);

        // Verify state in database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
        var dbConfig = await db.UserConfigurations.FirstOrDefaultAsync(c => c.User.UserName == username);
        dbConfig.Should().NotBeNull();
        dbConfig!.Theme.Should().Be(UserTheme.Light);
        dbConfig.Language.Should().Be(UserLanguage.Polish);
    }

    [Test]
    public async Task UpdateUserConfiguration_ShouldCreateConfigWithCustomValues_WhenUserHasNoConfig()
    {
        // Arrange
        var username = "updateconfig_custom";
        await RegisterAndLogin(username, "Password123!");

        var command = new UpdateUserConfigurationCommand
        {
            Theme = UserTheme.Dark,
            Language = UserLanguage.English
        };

        // Act
        var response = await _client.PutAsJsonAsync("/user/configuration", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserConfigurationDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Be(UserMessageConsts.UpdateUserConfigurationSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Theme.Should().Be(UserTheme.Dark);
        result.Data.Language.Should().Be(UserLanguage.English);

        // Verify state in database
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
        var dbConfig = await db.UserConfigurations.FirstOrDefaultAsync(c => c.User.UserName == username);
        dbConfig.Should().NotBeNull();
        dbConfig!.Theme.Should().Be(UserTheme.Dark);
        dbConfig.Language.Should().Be(UserLanguage.English);
    }

    [Test]
    public async Task UpdateUserConfiguration_ShouldUpdateThemeAndLanguage_WhenUserHasConfig()
    {
        // Arrange
        var username = "updateconfig_existing";
        await RegisterAndLogin(username, "Password123!");

        // First set to Light and Polish
        await _client.PutAsJsonAsync("/user/configuration", new UpdateUserConfigurationCommand
        {
            Theme = UserTheme.Light,
            Language = UserLanguage.Polish
        });

        // Act - Update both to Dark and English
        var command = new UpdateUserConfigurationCommand
        {
            Theme = UserTheme.Dark,
            Language = UserLanguage.English
        };
        var response = await _client.PutAsJsonAsync("/user/configuration", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserConfigurationDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Theme.Should().Be(UserTheme.Dark);
        result.Data.Language.Should().Be(UserLanguage.English);
    }

    [Test]
    public async Task UpdateUserConfiguration_ShouldUpdateOnlyTheme_WhenOnlyThemeProvided()
    {
        // Arrange
        var username = "updateconfig_themeonly";
        await RegisterAndLogin(username, "Password123!");

        // Set initial config to Light & Polish
        await _client.PutAsJsonAsync("/user/configuration", new UpdateUserConfigurationCommand
        {
            Theme = UserTheme.Light,
            Language = UserLanguage.Polish
        });

        // Act - Update only Theme to Dark
        var command = new UpdateUserConfigurationCommand
        {
            Theme = UserTheme.Dark,
            Language = null
        };
        var response = await _client.PutAsJsonAsync("/user/configuration", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserConfigurationDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Theme.Should().Be(UserTheme.Dark);
        result.Data.Language.Should().Be(UserLanguage.Polish); // preserved
    }

    [Test]
    public async Task UpdateUserConfiguration_ShouldUpdateOnlyLanguage_WhenOnlyLanguageProvided()
    {
        // Arrange
        var username = "updateconfig_langonly";
        await RegisterAndLogin(username, "Password123!");

        // Set initial config to Dark & Polish
        await _client.PutAsJsonAsync("/user/configuration", new UpdateUserConfigurationCommand
        {
            Theme = UserTheme.Dark,
            Language = UserLanguage.Polish
        });

        // Act - Update only Language to English
        var command = new UpdateUserConfigurationCommand
        {
            Theme = null,
            Language = UserLanguage.English
        };
        var response = await _client.PutAsJsonAsync("/user/configuration", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserConfigurationDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Theme.Should().Be(UserTheme.Dark); // preserved
        result.Data.Language.Should().Be(UserLanguage.English);
    }

    [Test]
    public async Task UpdateUserConfiguration_ShouldReturnIsCezConnectedTrue_WhenUserHasCezAccount()
    {
        // Arrange
        var username = "updateconfig_cezconnected";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var cezUser = new CezUser
            {
                UserId = userId,
                FullName = "Jan Kowalski",
                Token = "fake-token",
                PrivateToken = "fake-ptoken",
                ExternalUserId = 99999
            };
            db.CezUsers.Add(cezUser);
            await db.SaveChangesAsync();
        }

        var command = new UpdateUserConfigurationCommand
        {
            Theme = UserTheme.Dark,
            Language = UserLanguage.English
        };

        // Act
        var response = await _client.PutAsJsonAsync("/user/configuration", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserConfigurationDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsCezConnected.Should().BeTrue();
    }

    [Test]
    public async Task UpdateUserConfiguration_ShouldReturnIsCezConnectedFalse_WhenUserHasNoCezAccount()
    {
        // Arrange
        var username = "updateconfig_cezdisconnected";
        await RegisterAndLogin(username, "Password123!");

        var command = new UpdateUserConfigurationCommand
        {
            Theme = UserTheme.Dark,
            Language = UserLanguage.English
        };

        // Act
        var response = await _client.PutAsJsonAsync("/user/configuration", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserConfigurationDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsCezConnected.Should().BeFalse();
    }

    [Test]
    public async Task UpdateUserConfiguration_ShouldReturnStandardizedApiResponseStructure()
    {
        // Arrange
        var username = "updateconfig_apiresponse";
        await RegisterAndLogin(username, "Password123!");

        var command = new UpdateUserConfigurationCommand
        {
            Theme = UserTheme.Dark,
            Language = UserLanguage.Polish
        };

        // Act
        var response = await _client.PutAsJsonAsync("/user/configuration", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserConfigurationDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Be(UserMessageConsts.UpdateUserConfigurationSuccess);
        result.Data.Should().NotBeNull();
    }
}
