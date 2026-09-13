using CezStudentAssistant.Application.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace CezStudentAssistant.UnitTests.Application.Helpers;

[TestFixture]
public class TextNormalizationHelperTests
{
    [TestCase("Wiedźmin", "wiedzmin")]
    [TestCase("Zażółć gęślą jaźń", "zazolc gesla jazn")]
    [TestCase("Administracja", "administracja")]
    [TestCase("   ", "")]
    public void Normalize_ShouldRemoveDiacriticsAndConvertToLowercase(string input, string expected)
    {
        var result = TextNormalizationHelper.Normalize(input);
        result.Should().Be(expected);
    }

    [Test]
    public void Normalize_ShouldReturnEmptyString_WhenInputIsNull()
    {
        var result = TextNormalizationHelper.Normalize(null);
        result.Should().Be(string.Empty);
    }
}
