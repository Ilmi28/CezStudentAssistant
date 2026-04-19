using CezStudentAssistant.Domain.Interfaces.CQRS;

namespace CezStudentAssistant.Domain.Commands;

public class RegisterUserCommand : ICommand
{
    public required string UserName { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }
}
