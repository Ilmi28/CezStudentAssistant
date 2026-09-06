using CezStudentAssistant.Application.Commands.Quiz;
using FluentValidation;

namespace CezStudentAssistant.Application.Validators;

public class GenerateQuizCommandValidator : AbstractValidator<GenerateQuizCommand>
{
    public GenerateQuizCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();

        RuleFor(x => x.QuestionCount)
            .GreaterThan(0);

        RuleFor(x => x.TimeLimitMinutes)
            .GreaterThan(0)
            .When(x => x.TimeLimitMinutes.HasValue);

        RuleFor(x => x.AdditionalInstructions)
            .MaximumLength(5000)
            .When(x => !string.IsNullOrWhiteSpace(x.AdditionalInstructions));

        When(x => x.GenerateFromPromptOnly, () =>
        {
            RuleFor(x => x.AdditionalInstructions)
                .NotEmpty()
                .WithMessage("Własne instrukcje (prompt) są wymagane w trybie generowania bez plików kursu.");
        });
    }
}
