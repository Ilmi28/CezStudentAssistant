namespace CezStudentAssistant.Application.Responses;

public class BadGatewayResponse : ApiResponse
{
    public BadGatewayResponse(ApiMessage message)
        : base(false, System.Net.HttpStatusCode.BadGateway, message)
    {
    }
}
