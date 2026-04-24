using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Application.Responses;

public class UnauthorizedResponse : ApiResponse
{
    public UnauthorizedResponse(ApiMessage message)
    {
        Success = false;
        StatusCode = System.Net.HttpStatusCode.Unauthorized;
        Message = message.Message;
        ApplicationCode = $"{message.Source}_UNAUTHORIZED";
    }
}
