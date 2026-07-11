using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class Course : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public long? CezExternalId { get; set; }
    public DateTime LastSynched { get; set; } = DateTime.UtcNow;
    public CourseType Type { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
