using System.Net;

namespace CezStudentAssistant.Application.Responses;

public class ValidationResponse : ApiResponse
{
    public IEnumerable<ValidationError> Errors { get; set; }

    public ValidationResponse(ApiMessage message, IEnumerable<ValidationError> errors)
        : base(false, HttpStatusCode.BadRequest, message)
    {
        Errors = errors;
    }
}
