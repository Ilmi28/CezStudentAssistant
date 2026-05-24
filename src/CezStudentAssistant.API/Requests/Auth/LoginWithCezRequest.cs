namespace CezStudentAssistant.API.Requests.Auth;

public class LoginWithCezRequest
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}
