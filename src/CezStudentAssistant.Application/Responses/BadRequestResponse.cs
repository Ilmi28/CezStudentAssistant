namespace CezStudentAssistant.Application.Responses;

public class BadRequestResponse : ApiResponse
{
    public BadRequestResponse(ApiMessage message)
    {
        Success = false;
        StatusCode = System.Net.HttpStatusCode.BadRequest;
        Message = message.Message;
        ApplicationCode = $"{message.Source}_BAD_REQUEST";
    }
}
