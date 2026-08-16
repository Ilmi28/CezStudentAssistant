using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class Resource : BaseEntity
{
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public required string MimeType { get; set; }
    public ResourceSource Source { get; set; }
    public int EstimatedTokens { get; set; }
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public DateTime? CezLastModified { get; set; }
}
