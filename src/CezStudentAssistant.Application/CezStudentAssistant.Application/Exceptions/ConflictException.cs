using CezStudentAssistant.Application.Responses;

namespace CezStudentAssistant.Application.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(ApiMessage message) : base(message)
    {
        ApiMessage = message;
    }
}
