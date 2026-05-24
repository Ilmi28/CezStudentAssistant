using CezStudentAssistant.Application.Responses;

namespace CezStudentAssistant.Application.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(ApiMessage message) : base(message)
    {
        ApiMessage = message;
    }
}
