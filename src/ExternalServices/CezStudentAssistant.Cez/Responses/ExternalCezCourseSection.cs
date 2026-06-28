using System.Text.Json.Serialization;

namespace CezStudentAssistant.Cez.Responses;

internal class ExternalCezCourseSection
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("modules")]
    public List<ExternalModule> Modules { get; set; } = new List<ExternalModule>();
}

internal class ExternalModule
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("contextid")]
    public long ContextId { get; set; }

    [JsonPropertyName("contents")]
    public List<ExternalContent> Contents { get; set; } = new List<ExternalContent>();
}

internal class ExternalContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("mimetype")]
    public string? MimeType { get; set; }

    [JsonPropertyName("fileurl")]
    public required string FileUrl { get; set; }

    [JsonPropertyName("filename")]
    public required string FileName { get; set; }

    [JsonPropertyName("timecreated")]
    public long? TimeCreated { get; set; }

    [JsonPropertyName("timemodified")]
    public long TimeModified { get; set; }
}
