using CezStudentAssistant.Application.Commands.Flashcard;
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

namespace CezStudentAssistant.UnitTests.Commands;

[TestFixture]
public class UpdateFlashcardStateCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IFlashcardRepository _cardRepo = null!;
    private UpdateFlashcardStateCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _cardRepo = Substitute.For<IFlashcardRepository>();
        _unitOfWork.Repository<IFlashcardRepository>().Returns(_cardRepo);
        _handler = new UpdateFlashcardStateCommandHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_CardNotFound_ThrowsNotFoundException()
    {
        var cardId = Guid.NewGuid();
        var emptyMock = new List<Flashcard>().BuildMockDbSet();
        _cardRepo.Find(Arg.Any<Expression<Func<Flashcard, bool>>>())
            .Returns(emptyMock);

        var command = new UpdateFlashcardStateCommand
        {
            UserId = Guid.NewGuid(),
            CardId = cardId,
            State = FlashcardStateEnum.Mastered
        };

        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ExecuteAsync_DifferentUserId_ThrowsUnauthorizedException()
    {
        var cardId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var callerUserId = Guid.NewGuid();

        var card = new Flashcard
        {
            Id = cardId,
            Front = "Front",
            Back = "Back",
            Deck = new FlashcardDeck { Id = Guid.NewGuid(), UserId = ownerUserId, Name = "Deck" }
        };

        var mockList = new List<Flashcard> { card }.BuildMockDbSet();
        _cardRepo.Find(Arg.Any<Expression<Func<Flashcard, bool>>>())
            .Returns(mockList);

        var command = new UpdateFlashcardStateCommand
        {
            UserId = callerUserId,
            CardId = cardId,
            State = FlashcardStateEnum.Mastered
        };

        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test]
    public async Task ExecuteAsync_ValidRequest_UpdatesCardState()
    {
        var cardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var card = new Flashcard
        {
            Id = cardId,
            Front = "Front",
            Back = "Back",
            State = FlashcardStateEnum.New,
            Deck = new FlashcardDeck { Id = Guid.NewGuid(), UserId = userId, Name = "Deck" }
        };

        var mockList = new List<Flashcard> { card }.BuildMockDbSet();
        _cardRepo.Find(Arg.Any<Expression<Func<Flashcard, bool>>>())
            .Returns(mockList);

        var command = new UpdateFlashcardStateCommand
        {
            UserId = userId,
            CardId = cardId,
            State = FlashcardStateEnum.Mastered
        };

        var result = await _handler.Handle(command, CancellationToken.None);
        result.Success.Should().BeTrue();
        card.State.Should().Be(FlashcardStateEnum.Mastered);
        await _cardRepo.Received(1).UpdateAsync(card, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
