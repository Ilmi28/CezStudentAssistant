namespace CezStudentAssistant.Application.Dtos.Cez;

public class CezCourseContent
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long ModuleId { get; set; }
}
