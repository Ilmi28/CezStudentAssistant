using CezStudentAssistant.Application.Commands.Flashcard;
using FluentValidation;

namespace CezStudentAssistant.Application.Validators;

public class GenerateFlashcardsCommandValidator : AbstractValidator<GenerateFlashcardsCommand>
{
    public GenerateFlashcardsCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();

        RuleFor(x => x.CardCount)
            .GreaterThan(0)
            .LessThanOrEqualTo(50);

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
