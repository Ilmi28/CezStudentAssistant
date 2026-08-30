using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace CezStudentAssistant.UnitTests.Helpers;

[TestFixture]
public class FlashcardProgressCalculationHelperTests
{
    [Test]
    public void CalculateProgressPercentage_WithEmptyCards_ReturnsZero()
    {
        var result = FlashcardProgressCalculationHelper.CalculateProgressPercentage(new List<Flashcard>());
        result.Should().Be(0);
    }

    [Test]
    public void CalculateProgressPercentage_WithAllMasteredCards_Returns100()
    {
        var cards = new List<Flashcard>
        {
            new Flashcard { Front = "A", Back = "B", State = FlashcardStateEnum.Mastered },
            new Flashcard { Front = "C", Back = "D", State = FlashcardStateEnum.Mastered }
        };

        var result = FlashcardProgressCalculationHelper.CalculateProgressPercentage(cards);
        result.Should().Be(100);
    }

    [Test]
    public void CalculateProgressPercentage_WithMixedStates_CalculatesMasteredPercentage()
    {
        var cards = new List<Flashcard>
        {
            new Flashcard { Front = "A", Back = "B", State = FlashcardStateEnum.Mastered },
            new Flashcard { Front = "C", Back = "D", State = FlashcardStateEnum.Learning },
            new Flashcard { Front = "E", Back = "F", State = FlashcardStateEnum.New }
        };

        // 1 Mastered out of 3 = 33%
        var result = FlashcardProgressCalculationHelper.CalculateProgressPercentage(cards);
        result.Should().Be(33);
    }

    [Test]
    public void CalculateAttemptProgressPercentage_WithOneMasteredAndThreeLearning_Returns25()
    {
        var result = FlashcardProgressCalculationHelper.CalculateAttemptProgressPercentage(1, 3, 4);
        result.Should().Be(25);
    }
}
