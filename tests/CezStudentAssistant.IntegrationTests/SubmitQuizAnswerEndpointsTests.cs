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
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CezStudentAssistant.IntegrationTests;

[TestFixture]
public class SubmitQuizAnswerEndpointsTests
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
    public async Task SubmitAnswer_ShouldReturnUnauthorized_WhenNotLoggedIn()
    {
        var unauthorizedClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        var command = new SubmitQuizAnswerCommand
        {
            QuizAttemptId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            QuestionOptionIds = new List<Guid> { Guid.NewGuid() }
        };

        var response = await unauthorizedClient.PostAsJsonAsync("/quiz/answer", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task SubmitAnswer_ShouldReturnSuccessAndPersistAnswer_WhenRequestIsValid()
    {
        // Arrange
        var username = "quizuser1";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var courseId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var quizAttemptId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var course = new Course { Id = courseId, Name = "Course 1", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Id = quizId, UserId = userId, Name = "Quiz 1", DisplayName = "Quiz 1", Course = course };
            var question = new Question { Id = questionId, Quiz = quiz, Content = "Q1", Type = QuestionType.SingleChoice, Points = 5m };
            var option = new QuestionOption { Id = optionId, Question = question, Content = "Opt 1", IsCorrect = true };
            question.Options.Add(option);

            var attempt = new QuizAttempt
            {
                Id = quizAttemptId,
                User = user,
                Quiz = quiz,
                Course = course,
                Status = QuizAttemptStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            db.Courses.Add(course);
            db.Quizzes.Add(quiz);
            db.Questions.Add(question);
            db.QuestionOptions.Add(option);
            db.QuizAttempts.Add(attempt);

            await db.SaveChangesAsync();
        }

        var command = new SubmitQuizAnswerCommand
        {
            QuizAttemptId = quizAttemptId,
            QuestionId = questionId,
            QuestionOptionIds = new List<Guid> { optionId }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/answer", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>(_jsonOptions);
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Message.Should().Be(QuizMessageConsts.AnswerSubmittedSuccess);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var answer = await db.QuestionAnswers
                .Include(qa => qa.SelectedOptions)
                .FirstOrDefaultAsync(qa => qa.QuizAttemptId == quizAttemptId && qa.QuestionId == questionId);

            answer.Should().NotBeNull();
            answer.SelectedOptions.Should().HaveCount(1);
            answer.SelectedOptions.First().QuestionOptionId.Should().Be(optionId);
        }
    }

    [Test]
    public async Task SubmitAnswer_ShouldReturnNotFound_WhenQuizAttemptDoesNotExist()
    {
        // Arrange
        await RegisterAndLogin("quizuser2", "Password123!");

        var command = new SubmitQuizAnswerCommand
        {
            QuizAttemptId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(),
            QuestionOptionIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/answer", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SubmitAnswer_ShouldReturnForbidden_WhenQuizAttemptBelongsToOtherUser()
    {
        // Arrange
        var userA = "quizuser3a";
        var userB = "quizuser3b";
        
        await RegisterAndLogin(userB, "Password123!");
        var userBId = await GetCurrentUserIdFromDb(userB);

        var quizAttemptId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            
            var user = await db.Users.FirstAsync(u => u.Id == userBId);
            var course = new Course { Name = "Course B", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Name = "Quiz B", DisplayName = "Quiz B", Course = course, User = user };
            var attempt = new QuizAttempt
            {
                Id = quizAttemptId,
                User = user,
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

        // Login as User A
        await RegisterAndLogin(userA, "Password123!");

        var command = new SubmitQuizAnswerCommand
        {
            QuizAttemptId = quizAttemptId,
            QuestionId = Guid.NewGuid(),
            QuestionOptionIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/answer", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task SubmitAnswer_ShouldReturnBadRequest_WhenQuizAttemptCompleted()
    {
        // Arrange
        var username = "quizuser4";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var quizAttemptId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var course = new Course { Name = "Course A", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Name = "Quiz A", DisplayName = "Quiz A", Course = course, User = user };
            var attempt = new QuizAttempt
            {
                Id = quizAttemptId,
                User = user,
                Quiz = quiz,
                Course = course,
                Status = QuizAttemptStatus.Completed, // Already Completed
                StartedAt = DateTime.UtcNow
            };

            db.Courses.Add(course);
            db.Quizzes.Add(quiz);
            db.QuizAttempts.Add(attempt);
            await db.SaveChangesAsync();
        }

        var command = new SubmitQuizAnswerCommand
        {
            QuizAttemptId = quizAttemptId,
            QuestionId = Guid.NewGuid(),
            QuestionOptionIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/answer", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SubmitAnswer_ShouldReturnBadRequest_WhenQuizAttemptExpired()
    {
        // Arrange
        var username = "quizuser_expired";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var quizAttemptId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            
            var course = new Course { Name = "Course Expired", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Name = "Quiz Expired", DisplayName = "Quiz Expired", Course = course, User = user };
            var attempt = new QuizAttempt
            {
                Id = quizAttemptId,
                User = user,
                Quiz = quiz,
                Course = course,
                Status = QuizAttemptStatus.InProgress,
                StartedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddHours(-1)
            };

            db.Courses.Add(course);
            db.Quizzes.Add(quiz);
            db.QuizAttempts.Add(attempt);
            await db.SaveChangesAsync();
        }

        var command = new SubmitQuizAnswerCommand
        {
            QuizAttemptId = quizAttemptId,
            QuestionId = Guid.NewGuid(),
            QuestionOptionIds = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/answer", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SubmitAnswer_ShouldUpdateAnswer_WhenQuestionAlreadyAnswered()
    {
        // Arrange
        var username = "quizuser_answered";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var courseId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var oldOptionId = Guid.NewGuid();
        var newOptionId = Guid.NewGuid();
        var quizAttemptId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);

            var course = new Course { Id = courseId, Name = "Course Answered", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Id = quizId, UserId = userId, Name = "Quiz Answered", DisplayName = "Quiz Answered", Course = course };
            var question = new Question { Id = questionId, Quiz = quiz, Content = "Q Answered", Type = QuestionType.SingleChoice, Points = 5m };
            var oldOption = new QuestionOption { Id = oldOptionId, Question = question, Content = "Old Opt", IsCorrect = false };
            var newOption = new QuestionOption { Id = newOptionId, Question = question, Content = "New Opt", IsCorrect = true };
            question.Options.Add(oldOption);
            question.Options.Add(newOption);

            var attempt = new QuizAttempt
            {
                Id = quizAttemptId,
                User = user,
                Quiz = quiz,
                Course = course,
                Status = QuizAttemptStatus.InProgress,
                StartedAt = DateTime.UtcNow
            };

            var existingAnswer = new QuestionAnswer
            {
                QuizAttempt = attempt,
                Question = question,
                SelectedOptions = new List<SelectedQuizOption>
                {
                    new SelectedQuizOption { QuestionOption = oldOption }
                }
            };

            db.Courses.Add(course);
            db.Quizzes.Add(quiz);
            db.Questions.Add(question);
            db.QuestionOptions.AddRange(oldOption, newOption);
            db.QuizAttempts.Add(attempt);
            db.QuestionAnswers.Add(existingAnswer);
            await db.SaveChangesAsync();
        }

        var command = new SubmitQuizAnswerCommand
        {
            QuizAttemptId = quizAttemptId,
            QuestionId = questionId,
            QuestionOptionIds = new List<Guid> { newOptionId }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/answer", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var answer = await db.QuestionAnswers
                .Include(qa => qa.SelectedOptions)
                .FirstOrDefaultAsync(qa => qa.QuizAttemptId == quizAttemptId && qa.QuestionId == questionId);

            answer.Should().NotBeNull();
            answer.SelectedOptions.Should().HaveCount(1);
            answer.SelectedOptions.First().QuestionOptionId.Should().Be(newOptionId);
        }
    }

    [Test]
    public async Task SubmitAnswer_ShouldReturnBadRequest_WhenSingleChoiceQuestionHasMultipleOptions()
    {
        // Arrange
        var username = "quizuser_multsingle";
        await RegisterAndLogin(username, "Password123!");
        var userId = await GetCurrentUserIdFromDb(username);

        var courseId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var optionId1 = Guid.NewGuid();
        var optionId2 = Guid.NewGuid();
        var quizAttemptId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CezStudentAssistant.Infrastructure.Persistence.Data.AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == userId);

            var course = new Course { Id = courseId, Name = "Course Single", Type = CourseType.Cez };
            user.Courses.Add(course);

            var quiz = new Quiz { Id = quizId, UserId = userId, Name = "Quiz Single", DisplayName = "Quiz Single", Course = course };
            var question = new Question { Id = questionId, Quiz = quiz, Content = "Q Single", Type = QuestionType.SingleChoice, Points = 5m };
            var option1 = new QuestionOption { Id = optionId1, Question = question, Content = "Opt 1", IsCorrect = true };
            var option2 = new QuestionOption { Id = optionId2, Question = question, Content = "Opt 2", IsCorrect = false };
            question.Options.Add(option1);
            question.Options.Add(option2);

            var attempt = new QuizAttempt
            {
                Id = quizAttemptId,
                User = user,
                Quiz = quiz,
                Course = course,
                Status = QuizAttemptStatus.InProgress,
                StartedAt = DateTime.UtcNow
            };

            db.Courses.Add(course);
            db.Quizzes.Add(quiz);
            db.Questions.Add(question);
            db.QuestionOptions.AddRange(option1, option2);
            db.QuizAttempts.Add(attempt);
            await db.SaveChangesAsync();
        }

        var command = new SubmitQuizAnswerCommand
        {
            QuizAttemptId = quizAttemptId,
            QuestionId = questionId,
            QuestionOptionIds = new List<Guid> { optionId1, optionId2 }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/quiz/answer", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
