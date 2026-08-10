using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Consts;
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
public class StartQuizEndpointsTests
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
    public async Task StartQuiz_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        var unauthorizedClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        var command = new StartQuizCommand
        {
            QuizAttemptId = Guid.NewGuid()
        };

        var response = await unauthorizedClient.PostAsJsonAsync("/quiz/start", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task StartQuiz_ShouldReturnSuccessAndUpdateStatus_WhenRequestIsValid()
    {
        // Arrange
        var username = "startuser1";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var courseId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var quizAttemptId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var course = new Course { Id = courseId, Name = "Course 1", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Id = quizId, Name = "Quiz 1", DisplayName = "Quiz 1", Course = course };
            var attempt = new QuizAttempt
            {
                Id = quizAttemptId,
                User = user,
                Quiz = quiz,
                Course = course,
                Status = QuizAttemptStatus.NotStarted
            };

            db.Courses.Add(course);
            db.Quizzes.Add(quiz);
            db.QuizAttempts.Add(attempt);
            await db.SaveChangesAsync();
        }

        var command = new StartQuizCommand
        {
            QuizAttemptId = quizAttemptId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/start", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Message.Should().Be(QuizMessageConsts.StartQuizSuccess);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var attempt = await db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == quizAttemptId);

            attempt.Should().NotBeNull();
            attempt!.Status.Should().Be(QuizAttemptStatus.InProgress);
        }
    }

    [Test]
    public async Task StartQuiz_ShouldReturnNotFound_WhenQuizAttemptDoesNotExist()
    {
        // Arrange
        await RegisterAndLogin("startuser2", "Password123!");

        var command = new StartQuizCommand
        {
            QuizAttemptId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/start", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task StartQuiz_ShouldReturnForbidden_WhenQuizAttemptBelongsToOtherUser()
    {
        // Arrange
        var userA = "startuser3a";
        var userB = "startuser3b";
        
        await RegisterAndLogin(userB, "Password123!");
        var userBId = await GetCurrentUserIdFromDb(userB);

        var quizAttemptId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            
            var user = await db.Users.FirstAsync(u => u.Id == userBId);
            var course = new Course { Name = "Course B", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Name = "Quiz B", DisplayName = "Quiz B", Course = course };
            var attempt = new QuizAttempt
            {
                Id = quizAttemptId,
                User = user,
                Quiz = quiz,
                Course = course,
                Status = QuizAttemptStatus.NotStarted
            };

            db.Courses.Add(course);
            db.Quizzes.Add(quiz);
            db.QuizAttempts.Add(attempt);
            await db.SaveChangesAsync();
        }

        // Login as User A
        await RegisterAndLogin(userA, "Password123!");

        var command = new StartQuizCommand
        {
            QuizAttemptId = quizAttemptId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/start", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task StartQuiz_ShouldReturnBadRequest_WhenQuizAttemptAlreadyStarted()
    {
        // Arrange
        var username = "startuser4";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var quizAttemptId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var course = new Course { Name = "Course A", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Name = "Quiz A", DisplayName = "Quiz A", Course = course };
            var attempt = new QuizAttempt
            {
                Id = quizAttemptId,
                User = user,
                Quiz = quiz,
                Course = course,
                Status = QuizAttemptStatus.InProgress // Already started
            };

            db.Courses.Add(course);
            db.Quizzes.Add(quiz);
            db.QuizAttempts.Add(attempt);
            await db.SaveChangesAsync();
        }

        var command = new StartQuizCommand
        {
            QuizAttemptId = quizAttemptId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/start", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
