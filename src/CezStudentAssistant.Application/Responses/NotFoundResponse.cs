using System.Net;

namespace CezStudentAssistant.Application.Responses;

public class NotFoundResponse : ApiResponse
{
    public NotFoundResponse(ApiMessage message)
        : base(false, HttpStatusCode.NotFound, message)
    {
    }
}
