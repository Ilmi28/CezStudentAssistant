using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace CezStudentAssistant.UnitTests.Application.Helpers;

[TestFixture]
public class QuizPointHelperTests
{
    [TestCase(QuestionDifficulty.Easy, 1)]
    [TestCase(QuestionDifficulty.Medium, 2)]
    [TestCase(QuestionDifficulty.Hard, 3)]
    public void CalculatePoints_ShouldReturnExpectedPoints_ForGivenDifficulty(QuestionDifficulty difficulty, decimal expectedPoints)
    {
        // Act
        var result = QuizPointHelper.CalculatePoints(difficulty);

        // Assert
        result.Should().Be(expectedPoints);
    }

    [Test]
    public void CalculatePoints_ShouldFallbackToOnePoint_ForUndefinedDifficulty()
    {
        // Act
        var result = QuizPointHelper.CalculatePoints((QuestionDifficulty)999);

        // Assert
        result.Should().Be(1m);
    }
}
