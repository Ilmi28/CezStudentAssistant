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
    }
}
