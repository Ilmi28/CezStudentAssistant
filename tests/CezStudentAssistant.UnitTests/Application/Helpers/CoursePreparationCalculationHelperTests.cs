using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CezStudentAssistant.UnitTests.Application.Helpers;

[TestFixture]
public class CoursePreparationCalculationHelperTests
{
    [Test]
    public void CalculatePreparation_ShouldReturnNulls_WhenNoQuizzesOrDecksExist()
    {
        var result = CoursePreparationCalculationHelper.CalculatePreparation(new List<Quiz>(), new List<FlashcardDeck>());

        result.PreparationPercentage.Should().BeNull();
        result.QuizProgressPercentage.Should().BeNull();
        result.FlashcardProgressPercentage.Should().BeNull();
    }

    [Test]
    public void CalculatePreparation_ShouldCalculateWeightedFlashcardProgress_BasedOnCardDifficulty()
    {
        var deckId = Guid.NewGuid();
        var easyCard = new Flashcard
        {
            Id = Guid.NewGuid(),
            Front = "Q1",
            Back = "A1",
            Difficulty = QuestionDifficulty.Easy, // 1 pt
            State = FlashcardStateEnum.Mastered,
            DeckId = deckId
        };
        var hardCard = new Flashcard
        {
            Id = Guid.NewGuid(),
            Front = "Q2",
            Back = "A2",
            Difficulty = QuestionDifficulty.Hard, // 3 pts
            State = FlashcardStateEnum.New,
            DeckId = deckId
        };

        var deck = new FlashcardDeck
        {
            Id = deckId,
            Name = "Deck 1",
            Cards = new List<Flashcard> { easyCard, hardCard }
        };

        // Total weight = 1 + 3 = 4 pts. Mastered weight = 1 pt. 1/4 = 25%.
        var result = CoursePreparationCalculationHelper.CalculatePreparation(new List<Quiz>(), new List<FlashcardDeck> { deck });

        result.FlashcardProgressPercentage.Should().Be(25);
        result.QuizProgressPercentage.Should().BeNull();
        result.PreparationPercentage.Should().Be(25);
    }
}
