namespace CezStudentAssistant.Application.Dtos.Chat;

public class ChatThreadDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public required string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessageSnippet { get; set; }
    public List<Guid> AttachedResourceIds { get; set; } = [];
}
