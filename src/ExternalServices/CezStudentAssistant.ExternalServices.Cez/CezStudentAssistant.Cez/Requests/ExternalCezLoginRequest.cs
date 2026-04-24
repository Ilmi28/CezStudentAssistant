namespace CezStudentAssistant.Cez.Requests;

public class ExternalCezLoginRequest
{
    public required string UserName { get; set; }

    public required string Password { get; set; }

    public required string Service { get; set; }
}
