using CezStudentAssistant.Application.Commands.Flashcard;
using CezStudentAssistant.Application.Consts;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.UnitTests.Application.Commands.Flashcard;

[TestFixture]
public class DeleteFlashcardDeckCommandHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IFlashcardDeckRepository _deckRepository = null!;
    private ICascadeDeleteService _cascadeDeleteService = null!;
    private DeleteFlashcardDeckCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _deckRepository = Substitute.For<IFlashcardDeckRepository>();
        _cascadeDeleteService = Substitute.For<ICascadeDeleteService>();

        _unitOfWork.Repository<IFlashcardDeckRepository>().Returns(_deckRepository);
        _sut = new DeleteFlashcardDeckCommandHandler(_unitOfWork, _cascadeDeleteService);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldDeleteDeck_WhenUserIsOwner()
    {
        var deckId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var deck = new FlashcardDeck { Id = deckId, UserId = userId, Name = "Test Deck" };

        _deckRepository.GetByIdAsync(deckId, Arg.Any<CancellationToken>()).Returns(deck);

        var command = new DeleteFlashcardDeckCommand { UserId = userId, DeckId = deckId };

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        await _cascadeDeleteService.Received(1).DeleteFlashcardDeckCascadeAsync(deck, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_ShouldThrowNotFoundException_WhenDeckDoesNotExist()
    {
        var deckId = Guid.NewGuid();
        _deckRepository.GetByIdAsync(deckId, Arg.Any<CancellationToken>()).Returns((FlashcardDeck?)null);

        var command = new DeleteFlashcardDeckCommand { UserId = Guid.NewGuid(), DeckId = deckId };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        act.Should().ThrowAsync<NotFoundException>()
           .WithMessage(FlashcardMessageConsts.FlashcardDeckNotFound);
    }

    [Test]
    public void Handle_ShouldThrowUnauthorizedException_WhenUserIsNotOwner()
    {
        var deckId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var deck = new FlashcardDeck { Id = deckId, UserId = ownerId, Name = "Test Deck" };

        _deckRepository.GetByIdAsync(deckId, Arg.Any<CancellationToken>()).Returns(deck);

        var command = new DeleteFlashcardDeckCommand { UserId = callerId, DeckId = deckId };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        act.Should().ThrowAsync<UnauthorizedException>()
           .WithMessage(FlashcardMessageConsts.FlashcardAccessDenied);
    }
}
