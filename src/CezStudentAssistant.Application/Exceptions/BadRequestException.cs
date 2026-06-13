using CezStudentAssistant.Application.Responses;
namespace CezStudentAssistant.Application.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(ApiMessage message) : base(message)
    {
    }
}
