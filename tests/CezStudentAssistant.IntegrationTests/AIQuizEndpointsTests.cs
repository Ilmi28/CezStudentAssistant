using CezStudentAssistant.Application.Commands.Auth;
using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Dtos.Quiz;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.AI.Quiz;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using NSubstitute.ClearExtensions;

namespace CezStudentAssistant.IntegrationTests;

[TestFixture]
public class AIQuizEndpointsTests
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
    public async Task GenerateQuiz_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        var unauthorizedClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        var command = new GenerateQuizCommand
        {
            QuestionCount = 1,
            AdditionalInstructions = "test"
        };

        var response = await unauthorizedClient.PostAsJsonAsync($"/course/{Guid.NewGuid()}/generate-quiz", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GenerateQuiz_ShouldReturnNotFound_WhenCourseDoesNotExist()
    {
        var username = "quizuser_notfound";
        await RegisterAndLogin(username, "Password123!");

        var command = new GenerateQuizCommand
        {
            QuestionCount = 5
        };

        var response = await _client.PostAsJsonAsync($"/course/{Guid.NewGuid()}/generate-quiz", command);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GenerateQuiz_ShouldReturnBadRequest_WhenQuestionCountIsZero()
    {
        var username = "quizuser1";
        await RegisterAndLogin(username, "Password123!");

        var command = new GenerateQuizCommand
        {
            QuestionCount = 0
        };

        var response = await _client.PostAsJsonAsync($"/course/{Guid.NewGuid()}/generate-quiz", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GenerateQuiz_ShouldReturnBadRequestAndMarkJobAsFailed_WhenAIClientFails()
    {
        var username = "quizuser2";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var courseId = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            var course = new Course { Id = courseId, Name = "Failing AI Course", CezExternalId = 301, Type = CourseType.Cez };
            db.Courses.Add(course);
            user.Courses.Add(course);
            await db.SaveChangesAsync();
        }

        var command = new GenerateQuizCommand
        {
            QuestionCount = 5,
            AdditionalInstructions = "hard"
        };

        var response = await _client.PostAsJsonAsync($"/course/{courseId}/generate-quiz", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            
            var job = await db.Jobs.FirstOrDefaultAsync(j => j.UserId == userId && j.Type == JobType.QuizGeneration);
            job.Should().NotBeNull();
            job!.Status.Should().Be(JobStatus.Failed);
        }
    }

    [Test]
    public async Task GenerateQuiz_ShouldEnqueueAndCompleteQuizGeneration_WhenRequestIsValid()
    {
        var username = "quizuser3";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var courseId = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            var course = new Course { Id = courseId, Name = "AI Math Course", CezExternalId = 201, Type = CourseType.Cez };
            db.Courses.Add(course);
            user.Courses.Add(course);
            await db.SaveChangesAsync();
        }

        var command = new GenerateQuizCommand
        {
            QuestionCount = 1,
            AdditionalInstructions = "Make it easy"
        };

        var response = await _client.PostAsJsonAsync($"/course/{courseId}/generate-quiz", command);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TEST_ERROR_RESPONSE]: {body}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            
            var job = await db.Jobs.FirstOrDefaultAsync(j => j.UserId == userId && j.Type == JobType.QuizGeneration);
            job.Should().NotBeNull();
            job!.Status.Should().Be(JobStatus.Succeeded);

            var quiz = await db.Quizzes.Include(q => q.Questions).ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.CourseId == courseId);
            quiz.Should().NotBeNull();
            quiz!.Name.Should().Be("AI Math Quiz");
            quiz.Questions.Should().HaveCount(1);
            quiz.Questions.First().Content.Should().Be("What is 2+2?");
        }
    }

    [Test]
    public async Task GetQuizzes_ShouldReturnQuizzes_WhenUserHasQuizzes()
    {
        var username = "quizuser4";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var courseId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var course = new Course { Id = courseId, Name = "Course 10", Type = CourseType.Cez };
            db.Courses.Add(course);
            user.Courses.Add(course);

            var quiz = new Quiz { Id = quizId, UserId = userId, Name = "Calculus 1 Quiz", DisplayName = "Calculus 1 Quiz", CourseId = courseId };
            db.Quizzes.Add(quiz);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/quiz");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse<List<QuizDto>>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data.Should().HaveCount(1);
        content.Data![0].Id.Should().Be(quizId);
        content.Data[0].CourseName.Should().Be("Course 10");
    }

    [Test]
    public async Task GetQuizById_ShouldReturnDetails_WhenQuizExistsAndBelongsToUser()
    {
        var username = "quizuser5";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var courseId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var course = new Course { Id = courseId, Name = "Physics Course", Type = CourseType.Cez };
            db.Courses.Add(course);
            user.Courses.Add(course);

            var quiz = new Quiz { Id = quizId, UserId = userId, Name = "Electricity Quiz", DisplayName = "Electricity Quiz", CourseId = courseId };
            var question = new Question { Content = "V=IR?", Type = QuestionType.SingleChoice, Points = 1, Quiz = quiz };
            question.Options.Add(new QuestionOption { Content = "Yes", IsCorrect = true, Question = question });
            db.Quizzes.Add(quiz);
            db.Questions.Add(question);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/quiz/{quizId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse<QuizDetailsDto>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.Id.Should().Be(quizId);
        content.Data.Questions.Should().HaveCount(1);
        content.Data.Questions[0].Content.Should().Be("V=IR?");
        content.Data.Questions[0].Options.Should().HaveCount(1);
    }
}
