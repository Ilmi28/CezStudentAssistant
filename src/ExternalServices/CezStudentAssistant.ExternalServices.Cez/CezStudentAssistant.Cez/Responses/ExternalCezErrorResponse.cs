using System.Text.Json.Serialization;

namespace CezStudentAssistant.Cez.Responses;

public class ExternalCezErrorResponse
{
    [JsonPropertyName("error")]
    public required string Error { get; set; }

    [JsonPropertyName("errorcode")]
    public required string ErrorCode { get; set; }

    [JsonPropertyName("stacktrace")]
    public required string StackTrace { get; set; }

    [JsonPropertyName("debuginfo")]
    public required string DebugInfo { get; set; }

    [JsonPropertyName("reproductionlink")]
    public required string ReproductionLink { get; set; }
}
