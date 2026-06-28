using CezStudentAssistant.Application.Enums;

namespace CezStudentAssistant.Application.Dtos.Cez;

public class CezCourseContent
{
    public required string FileName { get; set; }
    public CezResourceType Type { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long ModuleId { get; set; }
    public DateTime TimeCreated { get; set; }
    public DateTime TimeModified { get; set; }
}
