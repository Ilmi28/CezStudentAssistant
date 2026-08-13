namespace CezStudentAssistant.Application.Dtos.Course;

public class CourseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTime LastSynched { get; set; }
    public bool IsCez { get; set; }
}
