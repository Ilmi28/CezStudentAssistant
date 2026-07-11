using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Application.Dtos.Course;

public class CourseDetailsDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public CourseType Type { get; set; }
    public DateTime LastSynched { get; set; }
    public List<CourseResourceDto> Files { get; set; } = new();
}
