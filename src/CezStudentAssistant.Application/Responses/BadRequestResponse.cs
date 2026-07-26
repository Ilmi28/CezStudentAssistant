namespace CezStudentAssistant.Application.Responses;

public class BadRequestResponse : ApiResponse
{
    public BadRequestResponse(ApiMessage message)
        : base(false, System.Net.HttpStatusCode.BadRequest, message)
    {
    }
}
