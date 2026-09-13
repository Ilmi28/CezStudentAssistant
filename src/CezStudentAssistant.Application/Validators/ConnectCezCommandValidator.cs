using CezStudentAssistant.Application.Commands.Cez;
using FluentValidation;

namespace CezStudentAssistant.Application.Validators;

public class ConnectCezCommandValidator : AbstractValidator<ConnectCezCommand>
{
    public ConnectCezCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
