using CezStudentAssistant.Application.Responses;

namespace CezStudentAssistant.Domain.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(ApiMessage message) : base(message)
    {
        ApiMessage = message;
    }
}
