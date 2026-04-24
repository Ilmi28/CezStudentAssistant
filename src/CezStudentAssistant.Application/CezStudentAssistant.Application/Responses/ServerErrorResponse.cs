using CezStudentAssistant.Domain.Responses;
using System.Net;

namespace CezStudentAssistant.Application.Responses;

public class ServerErrorResponse : ApiResponse
{
    public ServerErrorResponse()
    {
        Success = false;
        StatusCode = HttpStatusCode.InternalServerError;
        Message = "An unexpected error occurred while processing your request. Please try again later.";
        ApplicationCode = "INTERNAL_SERVER_ERROR";
    }
}
