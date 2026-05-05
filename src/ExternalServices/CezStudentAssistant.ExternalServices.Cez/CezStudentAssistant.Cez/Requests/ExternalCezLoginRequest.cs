namespace CezStudentAssistant.Cez.Requests;

internal class ExternalCezLoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
