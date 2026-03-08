using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Domain.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(ApiMessage message) : base(message)
    {
        ApiMessage = message;
    }
}
