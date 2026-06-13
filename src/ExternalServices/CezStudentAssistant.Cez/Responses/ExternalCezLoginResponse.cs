using System.Text.Json.Serialization;

namespace CezStudentAssistant.Cez.Responses;

internal class ExternalCezLoginResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }

    [JsonPropertyName("privatetoken")]
    public required string PrivateToken { get; set; }
}
