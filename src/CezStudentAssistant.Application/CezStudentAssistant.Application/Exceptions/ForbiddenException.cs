using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Exceptions;

namespace CezStudentAssistant.Application.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(ApiMessage message) : base(message)
    {
    }
}
