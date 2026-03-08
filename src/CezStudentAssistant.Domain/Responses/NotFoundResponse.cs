using System.Net;

namespace CezStudentAssistant.Domain.Responses;

public class NotFoundResponse : ApiResponse
{
    public NotFoundResponse(ApiMessage message)
    {
        Success = false;
        StatusCode = HttpStatusCode.NotFound;
        Message = message.Message;
        ApplicationCode = message.Code;
    }
}
