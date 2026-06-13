using System.Text.Json.Serialization;

namespace CezStudentAssistant.Cez.Responses;

internal class ExternalCezGetUserCoursesResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("shortname")]
    public string? ShortName { get; set; }

    [JsonPropertyName("fullname")]
    public string? FullName { get; set; }

    [JsonPropertyName("displayname")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("courseimage")]
    public string? CourseImage { get; set; }
}
