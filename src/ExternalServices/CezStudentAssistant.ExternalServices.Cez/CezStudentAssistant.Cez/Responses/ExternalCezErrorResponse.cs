using System.Text.Json.Serialization;

namespace CezStudentAssistant.Cez.Responses;

public class ExternalCezErrorResponse
{
    [JsonPropertyName("exception")]
    public required string Exception { get; set; }

    [JsonPropertyName("errorcode")]
    public required string ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("debuginfo")]
    public required string DebugInfo { get; set; }

}
