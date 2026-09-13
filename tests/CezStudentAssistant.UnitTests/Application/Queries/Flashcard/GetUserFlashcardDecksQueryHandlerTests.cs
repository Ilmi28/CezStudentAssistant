using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Queries.Flashcard;
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

namespace CezStudentAssistant.UnitTests.Application.Queries.Flashcard;

[TestFixture]
public class GetUserFlashcardDecksQueryHandlerTests
{
    private IUnitOfWork _unitOfWork = null!;
    private IFlashcardDeckRepository _deckRepository = null!;
    private GetUserFlashcardDecksQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _deckRepository = Substitute.For<IFlashcardDeckRepository>();

        _unitOfWork.Repository<IFlashcardDeckRepository>().Returns(_deckRepository);
        _sut = new GetUserFlashcardDecksQueryHandler(_unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork?.Dispose();
    }

    [Test]
    public async Task Handle_ShouldReturnPagedFlashcardDecks_WhenDecksExist()
    {
        var userId = Guid.NewGuid();
        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Deck 1",
            CreatedAt = DateTime.UtcNow,
            Cards = new List<Domain.Entities.Flashcard>(),
            Attempts = new List<FlashcardAttempt>()
        };

        var mockDbSet = new List<FlashcardDeck> { deck }.BuildMockDbSet();
        _deckRepository.Find(Arg.Any<Expression<Func<FlashcardDeck, bool>>>()).Returns(mockDbSet);

        var query = new GetUserFlashcardDecksQuery { UserId = userId, PageNumber = 1, PageSize = 10 };

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.TotalCount.Should().Be(1);
        result.Data.Items[0].Id.Should().Be(deck.Id);
    }
}
