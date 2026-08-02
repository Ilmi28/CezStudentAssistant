namespace CezStudentAssistant.Application.Responses;

public class BadGatewayResponse : ApiResponse
{
    public BadGatewayResponse(string message)
        : base(false, System.Net.HttpStatusCode.BadGateway, message)
    {
    }
}
