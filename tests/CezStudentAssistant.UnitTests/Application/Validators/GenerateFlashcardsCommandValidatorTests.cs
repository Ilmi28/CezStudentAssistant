using CezStudentAssistant.Application.Commands.Flashcard;
using CezStudentAssistant.Application.Validators;
using FluentValidation.TestHelper;
using NUnit.Framework;
using System;

namespace CezStudentAssistant.UnitTests.Application.Validators;

[TestFixture]
public class GenerateFlashcardsCommandValidatorTests
{
    private GenerateFlashcardsCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new GenerateFlashcardsCommandValidator();
    }

    [Test]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new GenerateFlashcardsCommand
        {
            CourseId = Guid.NewGuid(),
            CardCount = 10
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldFail_WhenCourseIdIsEmpty()
    {
        var command = new GenerateFlashcardsCommand
        {
            CourseId = Guid.Empty,
            CardCount = 10
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CourseId);
    }

    [Test]
    public void Validate_ShouldFail_WhenGenerateFromPromptOnlyIsTrueAndAdditionalInstructionsIsEmpty()
    {
        var command = new GenerateFlashcardsCommand
        {
            CourseId = Guid.NewGuid(),
            CardCount = 10,
            GenerateFromPromptOnly = true,
            AdditionalInstructions = ""
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AdditionalInstructions);
    }

    [Test]
    public void Validate_ShouldPass_WhenGenerateFromPromptOnlyIsTrueAndAdditionalInstructionsIsProvided()
    {
        var command = new GenerateFlashcardsCommand
        {
            CourseId = Guid.NewGuid(),
            CardCount = 10,
            GenerateFromPromptOnly = true,
            AdditionalInstructions = "Stwórz fiszki z sieci komputerowych"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
