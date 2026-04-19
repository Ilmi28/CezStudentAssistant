using CezStudentAssistant.Application.Responses;

namespace CezStudentAssistant.Domain.Exceptions;

public class AppException : Exception
{
    public ApiMessage ApiMessage { get; set; }

    public AppException(ApiMessage message) : base(message.Message)
    {
        ApiMessage = message;
    }

    public AppException(ApiMessage message, Exception innerException) : base(message.Message, innerException)
    {
        ApiMessage = message;
    }
}
