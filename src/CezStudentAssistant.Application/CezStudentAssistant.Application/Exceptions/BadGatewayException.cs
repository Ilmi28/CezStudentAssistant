using CezStudentAssistant.Application.Responses;
namespace CezStudentAssistant.Application.Exceptions;

public class BadGatewayException : AppException
{
    public BadGatewayException(ApiMessage message) : base(message)
    {
    }
}
