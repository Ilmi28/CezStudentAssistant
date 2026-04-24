namespace CezStudentAssistant.Application.Responses.Cez;

public class CezLoginResponse
{
    public required string Token { get; set; }

    public required string PrivateToken { get; set; }
}
