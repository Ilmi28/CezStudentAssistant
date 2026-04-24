using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Exceptions;

namespace CezStudentAssistant.Application.Exceptions;

public class BadGatewayException : AppException
{
    public BadGatewayException(ApiMessage message) : base(message)
    {
    }
}
