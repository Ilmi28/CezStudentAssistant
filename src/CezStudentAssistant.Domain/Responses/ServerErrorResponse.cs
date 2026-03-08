using System.Net;

namespace CezStudentAssistant.Domain.Responses;

public class ServerErrorResponse : ApiResponse
{
    public ServerErrorResponse(ApiMessage message)
    {
        Success = false;
        StatusCode = HttpStatusCode.InternalServerError;
        Message = message.Message;
        ApplicationCode = message.Code;
    }
}
