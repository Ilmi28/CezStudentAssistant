using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Domain.Exceptions;

public class AppException : Exception
{
    public ApiMessage ApiMessage { get; set; }

    public AppException()
    {

    }

    public AppException(string message) : base(message)
    {
    }

    public AppException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
