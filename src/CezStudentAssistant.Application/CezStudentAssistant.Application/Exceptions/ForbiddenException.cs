using CezStudentAssistant.Application.Responses;
namespace CezStudentAssistant.Application.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(ApiMessage message) : base(message)
    {
    }
}
