using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Validators;
using FluentValidation.TestHelper;
using NUnit.Framework;
using System;

namespace CezStudentAssistant.UnitTests.Application.Validators;

[TestFixture]
public class UpdateQuizCommandValidatorTests
{
    private UpdateQuizCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateQuizCommandValidator();
    }

    [Test]
    public void Validate_ShouldPass_WhenCommandIsValidWithoutTimeLimit()
    {
        var command = new UpdateQuizCommand
        {
            QuizId = Guid.NewGuid(),
            DisplayName = "Kolokwium 1",
            TimeLimitMinutes = null
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldPass_WhenCommandIsValidWithTimeLimit()
    {
        var command = new UpdateQuizCommand
        {
            QuizId = Guid.NewGuid(),
            DisplayName = "Kolokwium 1",
            TimeLimitMinutes = 45
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldFail_WhenQuizIdIsEmpty()
    {
        var command = new UpdateQuizCommand
        {
            QuizId = Guid.Empty,
            DisplayName = "Kolokwium 1"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.QuizId);
    }

    [Test]
    public void Validate_ShouldFail_WhenDisplayNameIsEmpty()
    {
        var command = new UpdateQuizCommand
        {
            QuizId = Guid.NewGuid(),
            DisplayName = ""
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }

    [Test]
    public void Validate_ShouldFail_WhenTimeLimitIsZeroOrNegative()
    {
        var command = new UpdateQuizCommand
        {
            QuizId = Guid.NewGuid(),
            DisplayName = "Kolokwium 1",
            TimeLimitMinutes = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TimeLimitMinutes);
    }
}
