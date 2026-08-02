namespace CezStudentAssistant.Application.Exceptions;

public class BadGatewayException : AppException
{
    public BadGatewayException(string message) : base(message)
    {
    }
}
