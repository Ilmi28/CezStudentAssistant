using System.Net;

namespace CezStudentAssistant.Application.Responses;

public class NotFoundResponse : ApiResponse
{
    public NotFoundResponse(ApiMessage message)
    {
        Success = false;
        StatusCode = HttpStatusCode.NotFound;
        Message = message.Message;
        ApplicationCode = $"{message.Source}_NOT_FOUND";
    }
}
