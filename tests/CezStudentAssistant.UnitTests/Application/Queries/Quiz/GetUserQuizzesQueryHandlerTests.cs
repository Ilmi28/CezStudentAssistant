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

namespace CezStudentAssistant.UnitTests.Application.Queries.QuizTests;

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
        var user = new User { Id = userId, UserName = "test" };
        var course = new Course { Id = Guid.NewGuid(), Name = "Course 1", Users = new List<User> { user } };
        var quiz = new Quiz { Id = Guid.NewGuid(), Name = "Quiz 1", DisplayName = "Quiz 1 Display", CourseId = course.Id, Course = course };

        var mockDbSet = new List<Quiz> { quiz }.BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<Quiz, bool>>>()).Returns(mockDbSet);

        var query = new GetUserQuizzesQuery { UserId = userId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Be(QuizMessageConsts.GetQuizzesSuccess);
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data![0].Name.Should().Be("Quiz 1");
        result.Data[0].CourseName.Should().Be("Course 1");
    }
}
