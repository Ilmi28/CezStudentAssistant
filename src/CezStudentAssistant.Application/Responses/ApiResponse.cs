using System.Net;

namespace CezStudentAssistant.Application.Responses;

public class ApiResponse
{
    public bool Success { get; set; }

    public HttpStatusCode StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public ApiResponse()
    {
    }

    protected ApiResponse(bool success, HttpStatusCode statusCode, string message)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
    }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public ApiResponse()
    {
    }

    protected ApiResponse(bool success, HttpStatusCode statusCode, string message, T? data = default)
        : base(success, statusCode, message)
    {
        Data = data;
    }
}

