using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Validators;
using FluentValidation.TestHelper;
using NUnit.Framework;
using System;

namespace CezStudentAssistant.UnitTests.Application.Validators;

[TestFixture]
public class GenerateQuizCommandValidatorTests
{
    private GenerateQuizCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new GenerateQuizCommandValidator();
    }

    [Test]
    public void Validate_ShouldPass_WhenCommandIsValidWithoutTimeLimit()
    {
        var command = new GenerateQuizCommand
        {
            CourseId = Guid.NewGuid(),
            QuestionCount = 5,
            TimeLimitMinutes = null
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldPass_WhenCommandIsValidWithPositiveTimeLimit()
    {
        var command = new GenerateQuizCommand
        {
            CourseId = Guid.NewGuid(),
            QuestionCount = 10,
            TimeLimitMinutes = 30
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldFail_WhenCourseIdIsEmpty()
    {
        var command = new GenerateQuizCommand
        {
            CourseId = Guid.Empty,
            QuestionCount = 5
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CourseId);
    }

    [Test]
    public void Validate_ShouldFail_WhenQuestionCountIsZeroOrNegative()
    {
        var command = new GenerateQuizCommand
        {
            CourseId = Guid.NewGuid(),
            QuestionCount = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.QuestionCount);
    }

    [Test]
    public void Validate_ShouldFail_WhenTimeLimitIsZeroOrNegative()
    {
        var command = new GenerateQuizCommand
        {
            CourseId = Guid.NewGuid(),
            QuestionCount = 5,
            TimeLimitMinutes = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TimeLimitMinutes);
    }

    [Test]
    public void Validate_ShouldFail_WhenGenerateFromPromptOnlyIsTrueAndAdditionalInstructionsIsEmpty()
    {
        var command = new GenerateQuizCommand
        {
            CourseId = Guid.NewGuid(),
            QuestionCount = 5,
            GenerateFromPromptOnly = true,
            AdditionalInstructions = ""
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AdditionalInstructions);
    }

    [Test]
    public void Validate_ShouldPass_WhenGenerateFromPromptOnlyIsTrueAndAdditionalInstructionsIsProvided()
    {
        var command = new GenerateQuizCommand
        {
            CourseId = Guid.NewGuid(),
            QuestionCount = 5,
            GenerateFromPromptOnly = true,
            AdditionalInstructions = "Stwórz quiz z BASH"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
