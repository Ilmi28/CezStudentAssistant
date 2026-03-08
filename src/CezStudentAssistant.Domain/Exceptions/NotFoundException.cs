using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Domain.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(ApiMessage message)
    {
        ApiMessage = message;
    }
}
