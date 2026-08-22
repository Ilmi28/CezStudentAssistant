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
    }
}
