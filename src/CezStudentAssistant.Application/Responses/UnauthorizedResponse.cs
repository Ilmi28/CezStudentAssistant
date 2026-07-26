namespace CezStudentAssistant.Application.Responses;

public class UnauthorizedResponse : ApiResponse
{
    public UnauthorizedResponse(ApiMessage message)
        : base(false, System.Net.HttpStatusCode.Unauthorized, message)
    {
    }
}
