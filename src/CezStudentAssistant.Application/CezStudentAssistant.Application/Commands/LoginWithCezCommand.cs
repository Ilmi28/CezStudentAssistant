using CezStudentAssistant.Domain.Interfaces.CQRS;

namespace CezStudentAssistant.Application.Commands;

public class LoginWithCezCommand : ICommand
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}
