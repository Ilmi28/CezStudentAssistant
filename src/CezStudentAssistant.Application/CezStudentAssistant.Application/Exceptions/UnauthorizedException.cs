using CezStudentAssistant.Application.Responses;
namespace CezStudentAssistant.Application.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(ApiMessage message) : base(message)
    {
    }
}
