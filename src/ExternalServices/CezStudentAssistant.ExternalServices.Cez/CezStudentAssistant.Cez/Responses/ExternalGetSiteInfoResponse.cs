using System.Text.Json.Serialization;

namespace CezStudentAssistant.Cez.Responses;

internal class ExternalGetSiteInfoResponse
{
    [JsonPropertyName("username")]
    public string? UserName { get; set; }

    [JsonPropertyName("fullname")]
    public string? FullName { get; set; }

    [JsonPropertyName("userid")]
    public long UserId { get; set; }
}
