using CezStudentAssistant.Application.Commands.Quiz;
using FluentValidation;

namespace CezStudentAssistant.Application.Validators;

public class UpdateQuizCommandValidator : AbstractValidator<UpdateQuizCommand>
{
    public UpdateQuizCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.TimeLimitMinutes)
            .GreaterThan(0)
            .When(x => x.TimeLimitMinutes.HasValue);
    }
}
