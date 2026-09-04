using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.Course;
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
using UserEntity = CezStudentAssistant.Domain.Entities.User;
using CourseEntity = CezStudentAssistant.Domain.Entities.Course;

namespace CezStudentAssistant.UnitTests.Application.Queries.Course;

[TestFixture]
public class GetCourseDetailsQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepository = null!;
    private ICezResourceRepository _resourceRepository = null!;
    private IQuizRepository _quizRepository = null!;
    private IQuizAttemptRepository _quizAttemptRepository = null!;
    private IFlashcardDeckRepository _flashcardDeckRepository = null!;
    private GetCourseDetailsQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _courseRepository = Substitute.For<ICourseRepository>();
        _resourceRepository = Substitute.For<ICezResourceRepository>();
        _quizRepository = Substitute.For<IQuizRepository>();
        _quizAttemptRepository = Substitute.For<IQuizAttemptRepository>();
        _flashcardDeckRepository = Substitute.For<IFlashcardDeckRepository>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepository);
        _unitOfWork.Repository<ICezResourceRepository>().Returns(_resourceRepository);
        _unitOfWork.Repository<IQuizRepository>().Returns(_quizRepository);
        _unitOfWork.Repository<IQuizAttemptRepository>().Returns(_quizAttemptRepository);
        _unitOfWork.Repository<IFlashcardDeckRepository>().Returns(_flashcardDeckRepository);

        _sut = new GetCourseDetailsQueryHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnCourseDetails_WhenCourseExistsAndBelongsToUser()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "student" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "Operating Systems",
            Description = "OS Course",
            Type = CourseType.User,
            Users = new List<UserEntity> { user }
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), true, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);

        var mockQuizzes = new List<Domain.Entities.Quiz>().BuildMockDbSet();
        _quizRepository.Find(Arg.Any<Expression<Func<Domain.Entities.Quiz, bool>>>(), true, Arg.Any<Expression<Func<Domain.Entities.Quiz, object>>[]>())
            .Returns(mockQuizzes);

        var mockAttempts = new List<QuizAttempt>().BuildMockDbSet();
        _quizAttemptRepository.Find(Arg.Any<Expression<Func<QuizAttempt, bool>>>(), true, Arg.Any<Expression<Func<QuizAttempt, object>>[]>())
            .Returns(mockAttempts);

        var mockDecks = new List<FlashcardDeck>().BuildMockDbSet();
        _flashcardDeckRepository.Find(Arg.Any<Expression<Func<FlashcardDeck, bool>>>(), true, Arg.Any<Expression<Func<FlashcardDeck, object>>[]>())
            .Returns(mockDecks);

        var query = new GetCourseDetailsQuery { UserId = userId, CourseId = courseId };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(courseId);
        result.Data.Files.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenCourseDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), true, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns((CourseEntity?)null);

        var query = new GetCourseDetailsQuery { UserId = userId, CourseId = courseId };

        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(CourseMessageConsts.CourseNotFound);
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedException_WhenUserDoesNotBelongToCourse()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var otherUser = new UserEntity { Id = otherUserId, UserName = "other" };
        var course = new CourseEntity
        {
            Id = courseId,
            Name = "OS",
            Users = new List<UserEntity> { otherUser }
        };

        _courseRepository.GetByIdAsync(courseId, Arg.Any<CancellationToken>(), true, Arg.Any<Expression<Func<CourseEntity, object>>[]>())
            .Returns(course);

        var query = new GetCourseDetailsQuery { UserId = userId, CourseId = courseId };

        Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage(CourseMessageConsts.CourseAccessDenied);
    }
}
