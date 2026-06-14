namespace CezStudentAssistant.Application.Interfaces.CQRS;

public interface IUserRequest
{
    Guid UserId { get; set; }
}
