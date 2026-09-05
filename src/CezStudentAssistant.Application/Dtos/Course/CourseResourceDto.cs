namespace CezStudentAssistant.Application.Dtos.Course;

public class CourseResourceDto
{
    public Guid Id { get; set; }
    public required string DisplayName { get; set; }
    public required string MimeType { get; set; }
    public DateTime LastModified { get; set; }
    public required string DownloadUrl { get; set; }
    public bool IsHidden { get; set; }
}
