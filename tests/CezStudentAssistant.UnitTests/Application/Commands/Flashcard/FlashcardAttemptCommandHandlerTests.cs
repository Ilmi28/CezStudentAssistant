using CezStudentAssistant.Application.Commands.Flashcard;
using CezStudentAssistant.Application.Dtos.Flashcard;
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
        var card1 = new FlashcardAttemptCard { FlashcardAttemptId = attemptId, FlashcardId = Guid.NewGuid(), State = FlashcardStateEnum.Mastered };
        var card2 = new FlashcardAttemptCard { FlashcardAttemptId = attemptId, FlashcardId = Guid.NewGuid(), State = FlashcardStateEnum.Learning };

        var attempt = new FlashcardAttempt
        {
            Id = attemptId,
            UserId = userId,
            CardCount = 2,
            Status = QuizAttemptStatus.InProgress,
            Cards = new List<FlashcardAttemptCard> { card1, card2 }
        };

        var mockAttemptDbSet = new List<FlashcardAttempt> { attempt }.BuildMockDbSet();
        _attemptRepo.Find(Arg.Any<Expression<Func<FlashcardAttempt, bool>>>(), Arg.Any<bool>()).Returns(mockAttemptDbSet);

        var handler = new CompleteFlashcardAttemptCommandHandler(_unitOfWork);
        var command = new CompleteFlashcardAttemptCommand { UserId = userId, AttemptId = attemptId };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data!.MasteredCount.Should().Be(1);
        result.Data.LearningCount.Should().Be(1);
        result.Data.Status.Should().Be(QuizAttemptStatus.Completed);
        await _attemptRepo.Received(1).UpdateAsync(Arg.Any<FlashcardAttempt>(), Arg.Any<CancellationToken>());
    }
}
