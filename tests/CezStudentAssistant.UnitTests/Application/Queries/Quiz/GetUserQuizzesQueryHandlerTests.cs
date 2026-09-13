using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.Quiz;
using CezStudentAssistant.Domain.Entities;
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
public class GetUserQuizzesQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IQuizRepository _quizRepository = null!;
    private GetUserQuizzesQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _quizRepository = Substitute.For<IQuizRepository>();

        _unitOfWork.Repository<IQuizRepository>().Returns(_quizRepository);
        _sut = new GetUserQuizzesQueryHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnQuizzes_WhenUserHasQuizzes()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "test" };
        var course = new CourseEntity { Id = Guid.NewGuid(), Name = "Course 1", Users = new List<UserEntity> { user } };
        var expiresAt = DateTime.UtcNow.AddMinutes(20);
        var quiz = new QuizEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Quiz 1",
            CourseId = course.Id,
            Course = course,
            TimeLimitMinutes = 25,
            Questions = new List<Question>
            {
                new Question
                {
                    Id = Guid.NewGuid(),
                    Content = "Q1",
                    Difficulty = CezStudentAssistant.Domain.Enums.QuestionDifficulty.Medium,
                    QuizId = Guid.NewGuid()
                }
            },
            Attempts = new List<QuizAttempt>
            {
                new QuizAttempt
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    QuizId = Guid.NewGuid(),
                    Status = CezStudentAssistant.Domain.Enums.QuizAttemptStatus.InProgress,
                    StartedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    Points = 5
                }
            }
        };

        var mockDbSet = new List<QuizEntity> { quiz }.BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<QuizEntity, bool>>>()).Returns(mockDbSet);

        var query = new GetUserQuizzesQuery { UserId = userId, PageNumber = 1, PageSize = 10 };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.TotalCount.Should().Be(1);
        result.Data.Items[0].Id.Should().Be(quiz.Id);
        result.Data.Items[0].TimeLimitMinutes.Should().Be(25);
        result.Data.Items[0].MaxPoints.Should().Be(2m);
        result.Data.Items[0].LastAttemptStatus.Should().Be(CezStudentAssistant.Domain.Enums.QuizAttemptStatus.InProgress);
        result.Data.Items[0].LastAttemptExpiresAt.Should().Be(expiresAt);
        result.Data.Items[0].LastAttemptPoints.Should().Be(5);
    }

    [Test]
    public async Task Handle_ShouldOrderQuizzesByCreatedAtDescending_WhenMultipleQuizzesExist()
    {
        var userId = Guid.NewGuid();
        var oldQuiz = new QuizEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Old Quiz",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            Questions = new List<Question>(),
            Attempts = new List<QuizAttempt>()
        };
        var newQuiz = new QuizEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "New Quiz",
            CreatedAt = DateTime.UtcNow,
            Questions = new List<Question>(),
            Attempts = new List<QuizAttempt>()
        };

        var mockDbSet = new List<QuizEntity> { oldQuiz, newQuiz }.BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<QuizEntity, bool>>>()).Returns(mockDbSet);

        var query = new GetUserQuizzesQuery { UserId = userId, PageNumber = 1, PageSize = 10 };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
        result.Data.Items[0].Id.Should().Be(newQuiz.Id);
        result.Data.Items[1].Id.Should().Be(oldQuiz.Id);
    }
}
