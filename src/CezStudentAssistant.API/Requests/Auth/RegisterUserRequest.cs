namespace CezStudentAssistant.API.Requests.Auth;

public class RegisterUserRequest
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}
