using CezStudentAssistant.Application.Commands.Flashcard;
using CezStudentAssistant.Application.Consts;
using FluentValidation;

namespace CezStudentAssistant.Application.Validators;

public class UpdateFlashcardDeckCommandValidator : AbstractValidator<UpdateFlashcardDeckCommand>
{
    public UpdateFlashcardDeckCommandValidator()
    {
        RuleFor(x => x.DeckId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(FlashcardMessageConsts.FlashcardDeckNameRequired)
            .MaximumLength(200);
    }
}
