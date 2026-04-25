using CezStudentAssistant.Application.Commands;
using FluentValidation;

namespace CezStudentAssistant.Application.Validators;

public class LoginWithCezCommandValidator : AbstractValidator<LoginWithCezCommand>
{
    public LoginWithCezCommandValidator()
    {
        RuleFor(x => x.UserName).NotNull();
        RuleFor(x => x.Password).NotNull();
    }
}
