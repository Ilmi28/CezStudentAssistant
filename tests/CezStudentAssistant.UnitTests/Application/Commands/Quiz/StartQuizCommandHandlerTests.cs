using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Dtos.Quiz;
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
public class StartQuizCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IQuizRepository _quizRepository = null!;
    private IQuizAttemptRepository _quizAttemptRepository = null!;
    private StartQuizCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _quizAttemptRepository = Substitute.For<IQuizAttemptRepository>();

        _unitOfWork.Repository<IQuizRepository>().Returns(_quizRepository);
        _unitOfWork.Repository<IQuizAttemptRepository>().Returns(_quizAttemptRepository);

        _sut = new StartQuizCommandHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldCreateAndStartAttempt_WhenQuizExistsAndBelongsToUser()
    {
        var userId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "student" };
        var course = new CourseEntity { Id = Guid.NewGuid(), Name = "Course 1", Users = new List<UserEntity> { user } };
        var quiz = new QuizEntity
        {
            Id = quizId,
            UserId = userId,
            Name = "Quiz 1",
            DisplayName = "Quiz Display 1",
            CourseId = course.Id,
            Course = course
        };
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Content = "What is 2+2?",
            Type = QuestionType.SingleChoice,
            QuizId = quizId,
            Quiz = quiz
        };
        var option = new QuestionOption { Id = Guid.NewGuid(), Content = "4", IsCorrect = true, QuestionId = question.Id, Question = question };
        question.Options.Add(option);
        quiz.Questions.Add(question);

        var mockQuizDbSet = new List<QuizEntity> { quiz }.BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<QuizEntity, bool>>>()).Returns(mockQuizDbSet);

        var command = new StartQuizCommand
        {
            UserId = userId,
            QuizId = quizId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(QuizMessageConsts.StartQuizSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.QuizId.Should().Be(quizId);
        result.Data.DisplayName.Should().Be("Quiz Display 1");
        result.Data.CourseName.Should().Be("Course 1");
        result.Data.Status.Should().Be(QuizAttemptStatus.InProgress);
        result.Data.IsPending.Should().BeTrue();
        result.Data.Questions.Should().HaveCount(1);
        result.Data.Answers.Should().BeEmpty();

        await _quizAttemptRepository.Received(1).AddAsync(Arg.Is<QuizAttempt>(a => a.QuizId == quizId && a.UserId == userId && a.Status == QuizAttemptStatus.InProgress), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenQuizDoesNotExist()
    {
        var mockQuizDbSet = new List<QuizEntity>().BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<QuizEntity, bool>>>()).Returns(mockQuizDbSet);

        var command = new StartQuizCommand
        {
            UserId = Guid.NewGuid(),
            QuizId = Guid.NewGuid()
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(QuizMessageConsts.QuizNotFound);
    }
}
