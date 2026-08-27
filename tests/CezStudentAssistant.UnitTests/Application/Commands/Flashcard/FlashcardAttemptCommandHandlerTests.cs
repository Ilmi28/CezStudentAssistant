using CezStudentAssistant.Application.Commands.Flashcard;
using CezStudentAssistant.Application.Dtos.Flashcard;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace CezStudentAssistant.UnitTests.Application.Commands.Flashcard;

[TestFixture]
public class FlashcardAttemptCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IFlashcardDeckRepository _deckRepo = null!;
    private IFlashcardAttemptRepository _attemptRepo = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _deckRepo = Substitute.For<IFlashcardDeckRepository>();
        _attemptRepo = Substitute.For<IFlashcardAttemptRepository>();

        _unitOfWork.Repository<IFlashcardDeckRepository>().Returns(_deckRepo);
        _unitOfWork.Repository<IFlashcardAttemptRepository>().Returns(_attemptRepo);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task StartFlashcardAttempt_ShouldCreateAttempt_WhenValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deckId = Guid.NewGuid();
        var deck = new FlashcardDeck { Id = deckId, UserId = userId, Name = "Test Deck" };

        _deckRepo.GetByIdAsync(deckId, Arg.Any<CancellationToken>()).Returns(deck);

        var handler = new StartFlashcardAttemptCommandHandler(_unitOfWork);
        var command = new StartFlashcardAttemptCommand { UserId = userId, DeckId = deckId, CardCount = 10 };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data!.DeckId.Should().Be(deckId);
        result.Data.CardCount.Should().Be(10);
        await _attemptRepo.Received(1).AddAsync(Arg.Any<FlashcardAttempt>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CompleteFlashcardAttempt_ShouldUpdateAttempt_WhenValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();
        var attempt = new FlashcardAttempt { Id = attemptId, UserId = userId, CardCount = 10, Status = QuizAttemptStatus.InProgress };

        _attemptRepo.GetByIdAsync(attemptId, Arg.Any<CancellationToken>()).Returns(attempt);

        var handler = new CompleteFlashcardAttemptCommandHandler(_unitOfWork);
        var command = new CompleteFlashcardAttemptCommand { UserId = userId, AttemptId = attemptId, MasteredCount = 7, LearningCount = 3 };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data!.MasteredCount.Should().Be(7);
        result.Data.LearningCount.Should().Be(3);
        result.Data.Status.Should().Be(QuizAttemptStatus.Completed);
        await _attemptRepo.Received(1).UpdateAsync(Arg.Any<FlashcardAttempt>(), Arg.Any<CancellationToken>());
    }
}
