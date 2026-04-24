namespace CezStudentAssistant.Application.Dtos.Cez;

public class CezSuccessLoginDto
{
    public required string Token { get; set; }

    public required string PrivateToken { get; set; }
}
