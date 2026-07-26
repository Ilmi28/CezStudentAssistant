namespace CezStudentAssistant.Application.Responses;

public class ForbiddenResponse : ApiResponse
{
    public ForbiddenResponse(ApiMessage message)
        : base(false, System.Net.HttpStatusCode.Forbidden, message)
    {
    }
}
