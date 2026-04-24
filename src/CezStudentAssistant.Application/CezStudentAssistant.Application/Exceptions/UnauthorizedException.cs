using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Exceptions;

namespace CezStudentAssistant.Application.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(ApiMessage message) : base(message)
    {
    }
}
