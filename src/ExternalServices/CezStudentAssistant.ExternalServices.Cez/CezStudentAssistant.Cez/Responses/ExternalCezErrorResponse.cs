using System.Text.Json.Serialization;

namespace CezStudentAssistant.Cez.Responses;

internal class ExternalCezErrorResponse
{
    [JsonPropertyName("exception")]
    public string? Exception { get; set; }

    [JsonPropertyName("errorcode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("debuginfo")]
    public string? DebugInfo { get; set; }
}
