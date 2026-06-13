using System.Net;

namespace CezStudentAssistant.Application.Responses;

public class ValidationResponse : ApiResponse
{
    public IEnumerable<ValidationError> Errors { get; set; }

    public ValidationResponse(ApiMessage message, IEnumerable<ValidationError> errors)
    {
        Success = false;
        StatusCode = HttpStatusCode.BadRequest;
        Message = message.Message;
        ApplicationCode = $"{message.Source}_VALIDATION";
        Errors = errors;
    }
}
