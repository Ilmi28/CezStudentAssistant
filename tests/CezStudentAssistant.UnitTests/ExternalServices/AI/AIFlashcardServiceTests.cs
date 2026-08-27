using CezStudentAssistant.AI.Services;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace CezStudentAssistant.UnitTests.ExternalServices.AI;

[TestFixture]
public class AIFlashcardServiceTests
{
    private ILogger<AIFlashcardService> _logger = null!;
    private AIFlashcardService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = Substitute.For<ILogger<AIFlashcardService>>();
        _service = new AIFlashcardService(_logger);
    }

    [Test]
    public void ParseResponse_ShouldParseValidJsonWithEnumStringsCorrectly()
    {
        // Arrange
        var json = """
            {
                "title": "Zestaw z Wiedźmina",
                "description": "Fiszki z wiedzy",
                "cards": [
                    {
                        "front": "Kim jest Geralt?",
                        "back": "Wiedźminem z Rivii",
                        "difficulty": "Easy"
                    },
                    {
                        "front": "Jak nazywa się miecz na potwory?",
                        "back": "Srebrny miecz",
                        "difficulty": "Medium"
                    }
                ]
            }
            """;

        // Act
        var result = _service.ParseResponse(json);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Zestaw z Wiedźmina");
        result.Cards.Should().HaveCount(2);
        result.Cards[0].Difficulty.Should().Be(QuestionDifficulty.Easy);
        result.Cards[1].Difficulty.Should().Be(QuestionDifficulty.Medium);
    }
}
