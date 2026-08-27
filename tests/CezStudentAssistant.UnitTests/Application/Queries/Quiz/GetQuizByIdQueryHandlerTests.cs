using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.Quiz;
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

namespace CezStudentAssistant.UnitTests.Application.Queries.Quiz;

using UserEntity = CezStudentAssistant.Domain.Entities.User;
using QuizEntity = CezStudentAssistant.Domain.Entities.Quiz;
using CourseEntity = CezStudentAssistant.Domain.Entities.Course;

[TestFixture]
public class GetQuizByIdQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IQuizRepository _quizRepository = null!;
    private IQuizAttemptRepository _quizAttemptRepository = null!;
    private GetQuizByIdQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _quizAttemptRepository = Substitute.For<IQuizAttemptRepository>();

        _unitOfWork.Repository<IQuizRepository>().Returns(_quizRepository);
        _unitOfWork.Repository<IQuizAttemptRepository>().Returns(_quizAttemptRepository);
        _sut = new GetQuizByIdQueryHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnQuizDetailsWithAttempts_WhenQuizExistsAndHasAttempts()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "test" };
        var course = new CourseEntity { Id = Guid.NewGuid(), Name = "Course A", Users = new List<UserEntity> { user } };
        var quizId = Guid.NewGuid();
        var quiz = new QuizEntity { Id = quizId, UserId = userId, Name = "Math Quiz", CourseId = course.Id, Course = course, TimeLimitMinutes = 15 };
        
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Content = "2+2?",
            Type = QuestionType.SingleChoice,
            Difficulty = QuestionDifficulty.Easy,
            QuizId = quizId,
            Quiz = quiz
        };
        var option = new QuestionOption { Id = Guid.NewGuid(), Content = "4", IsCorrect = true, QuestionId = question.Id, Question = question };
        question.Options.Add(option);
        quiz.Questions.Add(question);

        var attemptId = Guid.NewGuid();
        var answerId = Guid.NewGuid();
        var answer = new QuestionAnswer
        {
            Id = answerId,
            QuizAttemptId = attemptId,
            QuestionId = question.Id
        };
        answer.SelectedOptions.Add(new SelectedQuizOption
        {
            Id = Guid.NewGuid(),
            QuestionAnswerId = answerId,
            QuestionOptionId = option.Id
        });

        var attempt = new QuizAttempt
        {
            Id = attemptId,
            UserId = userId,
            QuizId = quizId,
            Status = QuizAttemptStatus.Completed,
            Points = 2.5m,
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAt = DateTime.UtcNow.AddMinutes(20),
            Answers = new List<QuestionAnswer> { answer }
        };

        var mockQuizDbSet = new List<QuizEntity> { quiz }.BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<QuizEntity, bool>>>()).Returns(mockQuizDbSet);

        var mockAttemptDbSet = new List<QuizAttempt> { attempt }.BuildMockDbSet();
        _quizAttemptRepository.Find(Arg.Any<Expression<Func<QuizAttempt, bool>>>()).Returns(mockAttemptDbSet);

        var query = new GetQuizByIdQuery { UserId = userId, QuizId = quizId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(quizId);
        result.Data.TimeLimitMinutes.Should().Be(15);
        result.Data.MaxPoints.Should().Be(1m);
        result.Data.Questions.Should().HaveCount(1);
        result.Data.Attempts.Should().HaveCount(1);
        result.Data.Attempts[0].Id.Should().Be(attemptId);
        result.Data.Attempts[0].Status.Should().Be(QuizAttemptStatus.Completed);
        result.Data.Attempts[0].Answers.Should().HaveCount(1);
        result.Data.Attempts[0].Answers[0].QuestionId.Should().Be(question.Id);
        result.Data.Attempts[0].Points.Should().Be(2.5m);
        result.Data.Attempts[0].Answers[0].SelectedOptionIds.Should().ContainSingle(id => id == option.Id);
    }

    [Test]
    public async Task Handle_ShouldReturnQuizDetailsWithEmptyAttempts_WhenQuizExistsAndHasNoAttempts()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "test" };
        var course = new CourseEntity { Id = Guid.NewGuid(), Name = "Course A", Users = new List<UserEntity> { user } };
        var quizId = Guid.NewGuid();
        var quiz = new QuizEntity { Id = quizId, UserId = userId, Name = "Math Quiz", CourseId = course.Id, Course = course };

        var mockQuizDbSet = new List<QuizEntity> { quiz }.BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<QuizEntity, bool>>>()).Returns(mockQuizDbSet);

        var mockAttemptDbSet = new List<QuizAttempt>().BuildMockDbSet();
        _quizAttemptRepository.Find(Arg.Any<Expression<Func<QuizAttempt, bool>>>()).Returns(mockAttemptDbSet);

        var query = new GetQuizByIdQuery { UserId = userId, QuizId = quizId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(quizId);
        result.Data.Attempts.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenQuizDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var quizId = Guid.NewGuid();

        var mockDbSet = new List<QuizEntity>().BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<QuizEntity, bool>>>()).Returns(mockDbSet);

        var query = new GetQuizByIdQuery { UserId = userId, QuizId = quizId };

        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(QuizMessageConsts.QuizNotFound);
    }
}
