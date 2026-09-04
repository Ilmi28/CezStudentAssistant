using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.User;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
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

namespace CezStudentAssistant.UnitTests.Application.Queries.User;

[TestFixture]
public class GetRecentActivityQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IQuizAttemptRepository _quizAttemptRepo = null!;
    private IFlashcardAttemptRepository _flashcardAttemptRepo = null!;
    private GetRecentActivityQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _quizAttemptRepo = Substitute.For<IQuizAttemptRepository>();
        _flashcardAttemptRepo = Substitute.For<IFlashcardAttemptRepository>();

        _unitOfWork.Repository<IQuizAttemptRepository>().Returns(_quizAttemptRepo);
        _unitOfWork.Repository<IFlashcardAttemptRepository>().Returns(_flashcardAttemptRepo);

        _sut = new GetRecentActivityQueryHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnCombinedRecentActivities_OrderedByAttemptDate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetRecentActivityQuery { UserId = userId, Limit = 5 };

        var course = new EntityCourse { Id = Guid.NewGuid(), Name = "Bazy Danych" };
        var quiz = new EntityQuiz { Id = Guid.NewGuid(), Name = "Quiz #1", CourseId = course.Id };
        var deck = new FlashcardDeck { Id = Guid.NewGuid(), Name = "Fiszki BD", CourseId = course.Id, Course = course };

        var quizAttempts = new List<QuizAttempt>
        {
            new QuizAttempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                QuizId = quiz.Id,
                Quiz = quiz,
                Course = course,
                StartedAt = DateTime.UtcNow.AddMinutes(-30),
                LastModifiedAt = DateTime.UtcNow.AddMinutes(-30),
                Points = 80,
                MaxPoints = 100,
                Status = QuizAttemptStatus.Completed
            }
        }.AsAsyncQueryable();

        var flashcardAttempts = new List<FlashcardAttempt>
        {
            new FlashcardAttempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeckId = deck.Id,
                Deck = deck,
                CardCount = 10,
                StartedAt = DateTime.UtcNow.AddMinutes(-10),
                LastModifiedAt = DateTime.UtcNow.AddMinutes(-10),
                Status = QuizAttemptStatus.Completed,
                Cards = new List<FlashcardAttemptCard>
                {
                    new FlashcardAttemptCard { State = FlashcardStateEnum.Mastered },
                    new FlashcardAttemptCard { State = FlashcardStateEnum.Mastered }
                }
            }
        }.AsAsyncQueryable();

        _quizAttemptRepo.Find(Arg.Any<Expression<Func<QuizAttempt, bool>>>(), Arg.Any<bool>(), Arg.Any<Expression<Func<QuizAttempt, object>>[]>())
            .Returns(quizAttempts);

        _flashcardAttemptRepo.Find(Arg.Any<Expression<Func<FlashcardAttempt, bool>>>(), Arg.Any<bool>(), Arg.Any<Expression<Func<FlashcardAttempt, object>>[]>())
            .Returns(flashcardAttempts);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(UserMessageConsts.GetRecentActivitySuccess);
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);

        // Flashcard attempt was started more recently (-10m vs -30m)
        result.Data![0].Type.Should().Be(ActivityType.Flashcard);
        result.Data[0].Title.Should().Be("Fiszki BD");
        result.Data[0].ScorePercentage.Should().Be(20.0);

        result.Data[1].Type.Should().Be(ActivityType.Quiz);
        result.Data[1].Title.Should().Be("Quiz #1");
        result.Data[1].ScorePercentage.Should().Be(80.0);
    }
}
