using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Exceptions;

namespace CezStudentAssistant.Application.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(ApiMessage message) : base(message)
    {
    }
}
