using System.Net;

namespace CezStudentAssistant.Application.Responses;

public class ConflictResponse : ApiResponse
{
    public ConflictResponse(string message)
        : base(false, HttpStatusCode.Conflict, message)
    {
    }
}
