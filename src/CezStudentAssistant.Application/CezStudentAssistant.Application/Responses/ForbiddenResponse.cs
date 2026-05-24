namespace CezStudentAssistant.Application.Responses;

public class ForbiddenResponse : ApiResponse
{
    public ForbiddenResponse(ApiMessage message)
    {
        Success = false;
        StatusCode = System.Net.HttpStatusCode.Forbidden;
        Message = message.Message;
        ApplicationCode = $"{message.Source}_FORBIDDEN";
    }
}
