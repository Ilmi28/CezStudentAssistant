namespace CezStudentAssistant.Application.Responses;

public class UnauthorizedResponse : ApiResponse
{
    public UnauthorizedResponse(string message)
        : base(false, System.Net.HttpStatusCode.Unauthorized, message)
    {
    }
}
