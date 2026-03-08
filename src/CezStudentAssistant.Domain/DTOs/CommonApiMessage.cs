using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Domain.DTOs;

public static class CommonApiMessage
{
    public readonly static ApiMessage AppServerError = new ApiMessage("SERVER_ERROR", "Unexpected server error occured.");
}
