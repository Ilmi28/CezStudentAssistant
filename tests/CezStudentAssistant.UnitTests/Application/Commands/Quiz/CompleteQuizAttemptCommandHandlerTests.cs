using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using MockQueryable.NSubstitute;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Commands.Quiz;

using UserEntity = CezStudentAssistant.Domain.Entities.User;
using QuizEntity = CezStudentAssistant.Domain.Entities.Quiz;
using CourseEntity = CezStudentAssistant.Domain.Entities.Course;

[TestFixture]
public class CompleteQuizAttemptCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IQuizAttemptRepository _quizAttemptRepository = null!;
    private CompleteQuizAttemptCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _quizAttemptRepository = Substitute.For<IQuizAttemptRepository>();

        _unitOfWork.Repository<IQuizAttemptRepository>().Returns(_quizAttemptRepository);
        _sut = new CompleteQuizAttemptCommandHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldCompleteAttemptAndCalculatePoints_WhenCommandIsValid()
    {
        var userId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var correctOptionId = Guid.NewGuid();

        var user = new UserEntity { Id = userId, UserName = "student" };
        var course = new CourseEntity { Id = Guid.NewGuid(), Name = "DB", Users = new List<UserEntity> { user } };
        var quiz = new QuizEntity { Id = quizId, UserId = userId, Name = "DB Quiz", Course = course };
        var question = new Question
        {
            Id = questionId,
            Content = "Is SQL relational?",
            Difficulty = QuestionDifficulty.Medium,
            Quiz = quiz,
            QuizId = quizId
        };
        var option = new QuestionOption
        {
            Id = correctOptionId,
            Content = "Yes",
            IsCorrect = true,
            Question = question,
            QuestionId = questionId
        };
        question.Options.Add(option);
        quiz.Questions.Add(question);

        var answer = new QuestionAnswer
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            QuizAttemptId = attemptId
        };
        answer.SelectedOptions.Add(new SelectedQuizOption
        {
            Id = Guid.NewGuid(),
            QuestionOptionId = correctOptionId,
            QuestionAnswerId = answer.Id
        });

        var attempt = new QuizAttempt
        {
            Id = attemptId,
            UserId = userId,
            QuizId = quizId,
            Quiz = quiz,
            Course = course,
            User = user,
            Status = QuizAttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            Answers = new List<QuestionAnswer> { answer }
        };

        var mockAttemptDbSet = new List<QuizAttempt> { attempt }.BuildMockDbSet();
        _quizAttemptRepository.Find(Arg.Any<Expression<Func<QuizAttempt, bool>>>()).Returns(mockAttemptDbSet);

        var command = new CompleteQuizAttemptCommand
        {
            UserId = userId,
            QuizAttemptId = attemptId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(QuizMessageConsts.CompleteQuizAttemptSuccess);

        attempt.Status.Should().Be(QuizAttemptStatus.Completed);
        attempt.Points.Should().Be(2m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenAttemptDoesNotExist()
    {
        var mockAttemptDbSet = new List<QuizAttempt>().BuildMockDbSet();
        _quizAttemptRepository.Find(Arg.Any<Expression<Func<QuizAttempt, bool>>>()).Returns(mockAttemptDbSet);

        var command = new CompleteQuizAttemptCommand
        {
            UserId = Guid.NewGuid(),
            QuizAttemptId = Guid.NewGuid()
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(QuizMessageConsts.QuizAttemptNotFound);
    }

    [Test]
    public async Task Handle_ShouldThrowBadRequestException_WhenAttemptIsNotInProgress()
    {
        var userId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();

        var attempt = new QuizAttempt
        {
            Id = attemptId,
            UserId = userId,
            Status = QuizAttemptStatus.Completed,
            StartedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        var mockAttemptDbSet = new List<QuizAttempt> { attempt }.BuildMockDbSet();
        _quizAttemptRepository.Find(Arg.Any<Expression<Func<QuizAttempt, bool>>>()).Returns(mockAttemptDbSet);

        var command = new CompleteQuizAttemptCommand
        {
            UserId = userId,
            QuizAttemptId = attemptId
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(QuizMessageConsts.QuizAttemptNotInProgress);
    }

    [Test]
    public async Task Handle_ShouldCompleteAttempt_WhenAttemptIsExpired()
    {
        var userId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "student" };
        var course = new CourseEntity { Id = Guid.NewGuid(), Name = "DB", Users = new List<UserEntity> { user } };
        var quiz = new QuizEntity { Id = quizId, UserId = userId, Name = "DB Quiz", Course = course };

        var attempt = new QuizAttempt
        {
            Id = attemptId,
            UserId = userId,
            QuizId = quizId,
            Quiz = quiz,
            Course = course,
            User = user,
            Status = QuizAttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow.AddMinutes(-60),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10)
        };

        var mockAttemptDbSet = new List<QuizAttempt> { attempt }.BuildMockDbSet();
        _quizAttemptRepository.Find(Arg.Any<Expression<Func<QuizAttempt, bool>>>()).Returns(mockAttemptDbSet);

        var command = new CompleteQuizAttemptCommand
        {
            UserId = userId,
            QuizAttemptId = attemptId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        attempt.Status.Should().Be(QuizAttemptStatus.Completed);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
