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
    public void CalculateProgressPercentage_WithMixedStates_CalculatesWeightedPercentage()
    {
        var cards = new List<Flashcard>
        {
            new Flashcard { Front = "A", Back = "B", State = FlashcardStateEnum.Mastered }, // 1.0
            new Flashcard { Front = "C", Back = "D", State = FlashcardStateEnum.Learning }, // 0.5
            new Flashcard { Front = "E", Back = "F", State = FlashcardStateEnum.New }       // 0.0
        };

        // Total weighted: 1.5 / 3 = 50%
        var result = FlashcardProgressCalculationHelper.CalculateProgressPercentage(cards);
        result.Should().Be(50);
    }
}
