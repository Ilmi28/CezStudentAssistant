using CezStudentAssistant.API.Requests.Auth;
using CezStudentAssistant.Application.Dtos.Auth;
using CezStudentAssistant.Application.Dtos.Cez;
using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CezStudentAssistant.IntegrationTests;

public class AuthEndpointsTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private CookieContainer _cookieContainer = null!;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _cookieContainer = new CookieContainer();
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
        // Arrange
        var request = new RegisterUserRequest { UserName = "testuser", Password = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<SuccessResponse<Guid>>(_jsonOptions);
        content!.Success.Should().BeTrue();

        response.Headers.Contains("Set-Cookie").Should().BeTrue();
        var cookies = response.Headers.GetValues("Set-Cookie");
        cookies.Should().Contain(c => c.Contains("accessToken"));
        cookies.Should().Contain(c => c.Contains("refreshToken"));
    }

    [Test]
    public async Task Login_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var registerRequest = new RegisterUserRequest { UserName = "loginuser", Password = "Password123!" };
        await _client.PostAsJsonAsync("/auth/register", registerRequest);
        
        var loginRequest = new LoginUserRequest { UserName = "loginuser", Password = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<SuccessResponse<Guid>>(_jsonOptions);
        content!.Success.Should().BeTrue();
        
        response.Headers.Contains("Set-Cookie").Should().BeTrue();
    }

    [Test]
    public async Task Refresh_ShouldIssueNewTokens_WhenValidRefreshTokenIsProvided()
    {
        // Arrange
        var registerRequest = new RegisterUserRequest { UserName = "refreshuser", Password = "Password123!" };
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", registerRequest);
        
        // HttpClient handles cookies automatically if configured, but by default it doesn't always preserve them between calls in simple setups? 
        // Actually, the default client from CreateClient() does NOT handle cookies automatically unless specified.
        // But for integration tests, it often works if it's the same client. Let's check.
        // Actually, I might need to manually extract cookies and add them to the next request or use a CookieContainer.

        // Act
        var response = await _client.PostAsync("/auth/refresh", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("Set-Cookie").Should().BeTrue();
    }

    [Test]
    public async Task Logout_ShouldClearCookies_WhenCalled()
    {
        // Arrange
        var registerRequest = new RegisterUserRequest { UserName = "logoutuser", Password = "Password123!" };
        await _client.PostAsJsonAsync("/auth/register", registerRequest);

        // Act
        var response = await _client.PostAsync("/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookies = response.Headers.GetValues("Set-Cookie");
        cookies.Should().Contain(c => c.Contains("accessToken=;")); // Expired cookie
    }

    [Test]
    public async Task LoginCez_ShouldReturnSuccess_WhenCezApiReturnsValidData()
    {
        // Arrange
        var request = new LoginWithCezRequest { UserName = "cezuser", Password = "cezpassword" };
        
        _factory.CezApiClientMock.LoginToCez(Arg.Any<CezLoginRequest>())
            .Returns(new CezLoginResponse 
            { 
                Success = true, 
                Data = new CezTokens { Token = "token_value", PrivateToken = "private_token_value" } 
            });

        _factory.CezApiClientMock.GetSiteInfo(Arg.Any<CezBaseRequest>())
            .Returns(new CezGetSiteInfoResponse 
            { 
                Success = true, 
                Data = new CezSiteInfo { UserName = "cezuser", FullName = "CEZ User", ExternalUserId = 123 } 
            });

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login-cez", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadFromJsonAsync<SuccessResponse<CezLoginResponse>>(_jsonOptions);
        content!.Success.Should().BeTrue();
        content.Data!.Data!.Token.Should().Be("token_value");
        
        response.Headers.Contains("Set-Cookie").Should().BeTrue();
    }
}
