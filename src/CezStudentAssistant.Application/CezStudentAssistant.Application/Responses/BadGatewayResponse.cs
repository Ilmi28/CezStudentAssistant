using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Application.Responses;

public class BadGatewayResponse : ApiResponse
{
    public BadGatewayResponse(ApiMessage message)
    {
        Success = false;
        StatusCode = System.Net.HttpStatusCode.BadGateway;
        Message = message.Message;
        ApplicationCode = $"{message.Source}_BAD_GATEWAY";
    }
}
