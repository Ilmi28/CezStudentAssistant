namespace CezStudentAssistant.Application.Requests.Cez;

public class CezLoginRequest
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}
