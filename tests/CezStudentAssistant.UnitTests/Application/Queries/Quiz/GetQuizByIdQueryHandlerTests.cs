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

namespace CezStudentAssistant.UnitTests.Application.Queries.QuizTests;

[TestFixture]
public class GetQuizByIdQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IQuizRepository _quizRepository = null!;
    private GetQuizByIdQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _unitOfWork.Repository<IQuizRepository>().Returns(_quizRepository);
        _sut = new GetQuizByIdQueryHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnQuizDetails_WhenQuizExistsAndBelongsToUser()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, UserName = "test" };
        var course = new Course { Id = Guid.NewGuid(), Name = "Course A", Users = new List<User> { user } };
        var quizId = Guid.NewGuid();
        var quiz = new Quiz { Id = quizId, Name = "Math Quiz", DisplayName = "Math Quiz Display", CourseId = course.Id, Course = course };
        
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Content = "2+2?",
            Type = QuestionType.SingleChoice,
            Points = 2.5m,
            QuizId = quizId,
            Quiz = quiz
        };
        var option = new QuestionOption { Id = Guid.NewGuid(), Content = "4", IsCorrect = true, QuestionId = question.Id, Question = question };
        question.Options.Add(option);
        quiz.Questions.Add(question);

        var mockDbSet = new List<Quiz> { quiz }.BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<Quiz, bool>>>()).Returns(mockDbSet);

        var query = new GetQuizByIdQuery { UserId = userId, QuizId = quizId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Be(QuizMessageConsts.GetQuizSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Math Quiz");
        result.Data.CourseName.Should().Be("Course A");
        result.Data.Questions.Should().HaveCount(1);
        result.Data.Questions[0].Content.Should().Be("2+2?");
        result.Data.Questions[0].Options.Should().HaveCount(1);
        result.Data.Questions[0].Options[0].Content.Should().Be("4");
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenQuizDoesNotExistOrAccessIsDenied()
    {
        var userId = Guid.NewGuid();
        var quizId = Guid.NewGuid();

        var mockDbSet = new List<Quiz>().BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<Quiz, bool>>>()).Returns(mockDbSet);

        var query = new GetQuizByIdQuery { UserId = userId, QuizId = quizId };

        Func<Task> act = () => _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{QuizMessageConsts.QuizNotFound}*");
    }
}
