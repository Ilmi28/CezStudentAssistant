using System.Net;

namespace CezStudentAssistant.Application.Responses;

public class ConflictResponse : ApiResponse
{
    public ConflictResponse(ApiMessage message)
    {
        Success = false;
        StatusCode = HttpStatusCode.Conflict;
        Message = message.Message;
        ApplicationCode = $"{message.Source}_{message.Message}";
    }
}
