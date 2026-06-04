using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class Course : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public long? CezExternalId { get; set; }
    public CourseType Type { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
