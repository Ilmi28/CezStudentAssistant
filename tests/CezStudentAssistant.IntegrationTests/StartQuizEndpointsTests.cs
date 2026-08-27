using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Quiz;
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

        var response = await unauthorizedClient.PostAsync($"/quiz/{Guid.NewGuid()}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task StartQuiz_ShouldCreateAttemptAndReturnDetails_WhenRequestIsValid()
    {
        var username = "startuser1";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var courseId = Guid.NewGuid();
        var quizId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var course = new Course { Id = courseId, Name = "Course 1", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Id = quizId, UserId = userId, Name = "Quiz 1", Course = course };
            var question = new Question { Content = "What is 2+2?", Type = QuestionType.SingleChoice, Quiz = quiz };
            var option = new QuestionOption { Content = "4", IsCorrect = true, Question = question };
            question.Options.Add(option);
            quiz.Questions.Add(question);

            db.Courses.Add(course);
            db.Quizzes.Add(quiz);
            db.Questions.Add(question);
            await db.SaveChangesAsync();
        }

        var response = await _client.PostAsync($"/quiz/{quizId}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<QuizAttemptDetailsDto>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Message.Should().Be(QuizMessageConsts.StartQuizSuccess);
        content.Data.Should().NotBeNull();
        content.Data!.QuizId.Should().Be(quizId);
        content.Data.Name.Should().Be("Quiz 1");
        content.Data.Status.Should().Be(QuizAttemptStatus.InProgress);
        content.Data.IsPending.Should().BeTrue();
        content.Data.Questions.Should().HaveCount(1);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var attempt = await db.QuizAttempts.FirstOrDefaultAsync(a => a.QuizId == quizId && a.UserId == userId);

            attempt.Should().NotBeNull();
            attempt!.Status.Should().Be(QuizAttemptStatus.InProgress);
        }
    }

    [Test]
    public async Task StartQuiz_ShouldReturnNotFound_WhenQuizDoesNotExist()
    {
        await RegisterAndLogin("startuser2", "Password123!");

        var response = await _client.PostAsync($"/quiz/{Guid.NewGuid()}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetQuizAttempt_ShouldReturnAttemptDetails_WhenAttemptExistsAndBelongsToUser()
    {
        var username = "attemptuser1";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var courseId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var course = new Course { Id = courseId, Name = "Data Structures", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Id = quizId, UserId = userId, Name = "Trees Quiz", Course = course };
            var question = new Question { Content = "Is binary tree hierarchical?", Type = QuestionType.SingleChoice, Quiz = quiz };
            var option = new QuestionOption { Content = "Yes", IsCorrect = true, Question = question };
            question.Options.Add(option);
            quiz.Questions.Add(question);

            var attempt = new QuizAttempt
            {
                Id = attemptId,
                UserId = userId,
                Quiz = quiz,
                Course = course,
                Status = QuizAttemptStatus.InProgress,
                StartedAt = DateTime.UtcNow
            };
            var answer = new QuestionAnswer
            {
                QuizAttempt = attempt,
                Question = question
            };
            answer.SelectedOptions.Add(new SelectedQuizOption
            {
                QuestionAnswer = answer,
                QuestionOption = option
            });
            attempt.Answers.Add(answer);

            db.Courses.Add(course);
            db.Quizzes.Add(quiz);
            db.Questions.Add(question);
            db.QuizAttempts.Add(attempt);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/quiz/attempt/{attemptId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<QuizAttemptDetailsDto>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.AttemptId.Should().Be(attemptId);
        content.Data.QuizId.Should().Be(quizId);
        content.Data.Name.Should().Be("Trees Quiz");
        content.Data.Status.Should().Be(QuizAttemptStatus.InProgress);
        content.Data.IsPending.Should().BeTrue();
        content.Data.Questions.Should().HaveCount(1);
        content.Data.Answers.Should().HaveCount(1);
        content.Data.Points.Should().BeNull();
    }

    [Test]
    public async Task CompleteQuizAttempt_ShouldUpdateStatusToCompleted_WhenRequestIsValid()
    {
        var username = "completeuser1";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var courseId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var course = new Course { Id = courseId, Name = "Networks", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Id = quizId, UserId = userId, Name = "Net Quiz", Course = course };
            var attempt = new QuizAttempt
            {
                Id = attemptId,
                UserId = userId,
                Quiz = quiz,
                Course = course,
                Status = QuizAttemptStatus.InProgress,
                StartedAt = DateTime.UtcNow
            };

            db.Courses.Add(course);
            db.Quizzes.Add(quiz);
            db.QuizAttempts.Add(attempt);
            await db.SaveChangesAsync();
        }

        var response = await _client.PostAsync($"/quiz/attempt/{attemptId}/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Message.Should().Be(QuizMessageConsts.CompleteQuizAttemptSuccess);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var attempt = await db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == attemptId);

            attempt.Should().NotBeNull();
            attempt!.Status.Should().Be(QuizAttemptStatus.Completed);
        }
    }
}
