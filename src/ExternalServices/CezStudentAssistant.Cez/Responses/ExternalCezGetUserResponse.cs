using System.Text.Json.Serialization;

namespace CezStudentAssistant.Cez.Responses;

internal class ExternalCezGetUserResponse
{
    [JsonPropertyName("id")]
    public long UserId { get; set; }

    [JsonPropertyName("username")]
    public string? UserName { get; set; }

    [JsonPropertyName("fullname")]
    public string? FullName { get; set; }

    [JsonPropertyName("firstname")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastname")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    public string? EffectiveFullName => !string.IsNullOrWhiteSpace(FullName) ? FullName : (!string.IsNullOrWhiteSpace(FirstName) ? $"{FirstName} {LastName}".Trim() : null);
}
