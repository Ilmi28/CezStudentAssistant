namespace CezStudentAssistant.Application.Dtos.Course;

public class FileResultDto
{
    public required Stream FileStream { get; set; }
    public required string ContentType { get; set; }
    public required string FileName { get; set; }
}
