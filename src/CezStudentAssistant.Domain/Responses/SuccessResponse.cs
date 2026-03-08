using System.Net;

namespace CezStudentAssistant.Domain.Responses;

public class SuccessResponse<T> : ApiResponse<T>
{
    public SuccessResponse(ApiMessage message, T data)
    {
        Success = true;
        StatusCode = HttpStatusCode.OK;
        Message = message.Message;
        Data = data;
        ApplicationCode = message.Code;
    }
}

public class SuccessResponse : ApiResponse
{
    public SuccessResponse(ApiMessage message)
    {
        Success = true;
        StatusCode = HttpStatusCode.OK;
        Message = message.Message;
        ApplicationCode = message.Code;
    }
}
