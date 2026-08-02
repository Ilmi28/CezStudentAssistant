namespace CezStudentAssistant.Application.Responses;

public class BadRequestResponse : ApiResponse
{
    public BadRequestResponse(string message)
        : base(false, System.Net.HttpStatusCode.BadRequest, message)
    {
    }
}
