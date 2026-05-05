namespace CezStudentAssistant.Application.Requests.Cez;

public class CezBaseRequest
{
    public required string Token { get; set; }
    public required string Function { get; set; }
}
