using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.User;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CezStudentAssistant.UnitTests.Helpers;
using EntityCourse = CezStudentAssistant.Domain.Entities.Course;
using EntityQuiz = CezStudentAssistant.Domain.Entities.Quiz;
using EntityUser = CezStudentAssistant.Domain.Entities.User;
using EntityFlashcard = CezStudentAssistant.Domain.Entities.Flashcard;

namespace CezStudentAssistant.UnitTests.Application.Queries.User;

[TestFixture]
public class GetDashboardStatsQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private ICourseRepository _courseRepo = null!;
    private IQuizRepository _quizRepo = null!;
    private IFlashcardDeckRepository _deckRepo = null!;
    private IFlashcardRepository _cardRepo = null!;
    private GetDashboardStatsQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _courseRepo = Substitute.For<ICourseRepository>();
        _quizRepo = Substitute.For<IQuizRepository>();
        _deckRepo = Substitute.For<IFlashcardDeckRepository>();
        _cardRepo = Substitute.For<IFlashcardRepository>();

        _unitOfWork.Repository<ICourseRepository>().Returns(_courseRepo);
        _unitOfWork.Repository<IQuizRepository>().Returns(_quizRepo);
        _unitOfWork.Repository<IFlashcardDeckRepository>().Returns(_deckRepo);
        _unitOfWork.Repository<IFlashcardRepository>().Returns(_cardRepo);

        _sut = new GetDashboardStatsQueryHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnCounts_WhenQueriesExecuted()
    {
        var userId = Guid.NewGuid();
        var query = new GetDashboardStatsQuery { UserId = userId };

        var courses = new List<EntityCourse>
        {
            new EntityCourse { Id = Guid.NewGuid(), Name = "C1", Users = new List<EntityUser> { new EntityUser { Id = userId, UserName = "testuser" } } }
        }.AsAsyncQueryable();

        var quizzes = new List<EntityQuiz>
        {
            new EntityQuiz { Id = Guid.NewGuid(), Name = "Q1", UserId = userId },
            new EntityQuiz { Id = Guid.NewGuid(), Name = "Q2", UserId = userId }
        }.AsAsyncQueryable();

        var decks = new List<FlashcardDeck>
        {
            new FlashcardDeck { Id = Guid.NewGuid(), Name = "D1", UserId = userId }
        }.AsAsyncQueryable();

        var deckId = Guid.NewGuid();
        var cards = new List<EntityFlashcard>
        {
            new EntityFlashcard { Id = Guid.NewGuid(), Front = "T1", Back = "D1", DeckId = deckId, Deck = new FlashcardDeck { Name = "D1", UserId = userId } },
            new EntityFlashcard { Id = Guid.NewGuid(), Front = "T2", Back = "D2", DeckId = deckId, Deck = new FlashcardDeck { Name = "D1", UserId = userId } },
            new EntityFlashcard { Id = Guid.NewGuid(), Front = "T3", Back = "D3", DeckId = deckId, Deck = new FlashcardDeck { Name = "D1", UserId = userId } }
        }.AsAsyncQueryable();

        _courseRepo.Find(Arg.Any<Expression<Func<EntityCourse, bool>>>(), Arg.Any<bool>(), Arg.Any<Expression<Func<EntityCourse, object>>[]>())
            .Returns(courses);

        _quizRepo.Find(Arg.Any<Expression<Func<EntityQuiz, bool>>>(), Arg.Any<bool>(), Arg.Any<Expression<Func<EntityQuiz, object>>[]>())
            .Returns(quizzes);

        _deckRepo.Find(Arg.Any<Expression<Func<FlashcardDeck, bool>>>(), Arg.Any<bool>(), Arg.Any<Expression<Func<FlashcardDeck, object>>[]>())
            .Returns(decks);

        _cardRepo.Find(Arg.Any<Expression<Func<EntityFlashcard, bool>>>(), Arg.Any<bool>(), Arg.Any<Expression<Func<EntityFlashcard, object>>[]>())
            .Returns(cards);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(UserMessageConsts.GetDashboardStatsSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.CourseCount.Should().Be(1);
        result.Data.QuizCount.Should().Be(2);
        result.Data.FlashcardDeckCount.Should().Be(1);
        result.Data.FlashcardCount.Should().Be(3);
    }
}
