namespace CezStudentAssistant.Domain.Entities;

public class CezResource : BaseEntity
{
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public required string MimeType { get; set; }
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public DateTime CezLastModified { get; set; }
}
