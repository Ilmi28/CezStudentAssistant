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
public class GetQuizAttemptByIdQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IQuizAttemptRepository _quizAttemptRepository = null!;
    private GetQuizAttemptByIdQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _quizAttemptRepository = Substitute.For<IQuizAttemptRepository>();

        _unitOfWork.Repository<IQuizAttemptRepository>().Returns(_quizAttemptRepository);
        _sut = new GetQuizAttemptByIdQueryHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnQuizAttemptDetails_WhenAttemptExistsAndBelongsToUser()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "student" };
        var course = new CourseEntity { Id = Guid.NewGuid(), Name = "Operating Systems", Users = new List<UserEntity> { user } };
        var quizId = Guid.NewGuid();
        var quiz = new QuizEntity
        {
            Id = quizId,
            UserId = userId,
            Name = "OS Quiz",
            CourseId = course.Id,
            Course = course
        };
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Content = "What is a process?",
            Type = QuestionType.SingleChoice,
            QuizId = quizId,
            Quiz = quiz
        };
        var option = new QuestionOption { Id = Guid.NewGuid(), Content = "Program in execution", IsCorrect = true, QuestionId = question.Id, Question = question };
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
            Quiz = quiz,
            Course = course,
            User = user,
            Status = QuizAttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddMinutes(25),
            Answers = new List<QuestionAnswer> { answer }
        };

        var mockAttemptDbSet = new List<QuizAttempt> { attempt }.BuildMockDbSet();
        _quizAttemptRepository.Find(Arg.Any<Expression<Func<QuizAttempt, bool>>>()).Returns(mockAttemptDbSet);

        var query = new GetQuizAttemptByIdQuery
        {
            UserId = userId,
            QuizAttemptId = attemptId
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(QuizMessageConsts.GetQuizAttemptSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.AttemptId.Should().Be(attemptId);
        result.Data.QuizId.Should().Be(quizId);
        result.Data.Name.Should().Be("OS Quiz");
        result.Data.CourseName.Should().Be("Operating Systems");
        result.Data.Status.Should().Be(QuizAttemptStatus.InProgress);
        result.Data.IsPending.Should().BeTrue();
        result.Data.Questions.Should().HaveCount(1);
        result.Data.Questions[0].Content.Should().Be("What is a process?");
        result.Data.Answers.Should().HaveCount(1);
        result.Data.Answers[0].QuestionId.Should().Be(question.Id);
        result.Data.Answers[0].SelectedOptionIds.Should().ContainSingle(id => id == option.Id);
        result.Data.Points.Should().BeNull();
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenAttemptDoesNotExist()
    {
        var mockAttemptDbSet = new List<QuizAttempt>().BuildMockDbSet();
        _quizAttemptRepository.Find(Arg.Any<Expression<Func<QuizAttempt, bool>>>()).Returns(mockAttemptDbSet);

        var query = new GetQuizAttemptByIdQuery
        {
            UserId = Guid.NewGuid(),
            QuizAttemptId = Guid.NewGuid()
        };

        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(QuizMessageConsts.QuizAttemptNotFound);
    }
}
