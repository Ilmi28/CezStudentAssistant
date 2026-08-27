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
    }
}
