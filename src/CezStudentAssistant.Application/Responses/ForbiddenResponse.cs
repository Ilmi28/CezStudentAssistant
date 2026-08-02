namespace CezStudentAssistant.Application.Responses;

public class ForbiddenResponse : ApiResponse
{
    public ForbiddenResponse(string message)
        : base(false, System.Net.HttpStatusCode.Forbidden, message)
    {
    }
}
