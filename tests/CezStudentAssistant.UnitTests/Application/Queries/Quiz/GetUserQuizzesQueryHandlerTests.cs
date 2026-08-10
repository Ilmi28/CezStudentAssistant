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
        var quiz = new QuizEntity { Id = Guid.NewGuid(), Name = "Quiz 1", DisplayName = "Quiz 1 Display", CourseId = course.Id, Course = course };

        var mockDbSet = new List<QuizEntity> { quiz }.BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<QuizEntity, bool>>>()).Returns(mockDbSet);

        var query = new GetUserQuizzesQuery { UserId = userId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].Id.Should().Be(quiz.Id);
    }
}
