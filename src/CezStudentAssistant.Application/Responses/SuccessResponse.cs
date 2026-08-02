using System.Net;

namespace CezStudentAssistant.Application.Responses;

public class SuccessResponse<T> : ApiResponse<T>
{
    public SuccessResponse() 
    {
        Success = true;
        StatusCode = HttpStatusCode.OK;
    }

    public SuccessResponse(string message, T data)
        : base(true, HttpStatusCode.OK, message, data)
    {
    }
}

public class SuccessResponse : ApiResponse
{
    public SuccessResponse()
    {
        Success = true;
        StatusCode = HttpStatusCode.OK;
    }

    public SuccessResponse(string message)
        : base(true, HttpStatusCode.OK, message)
    {
    }
}
