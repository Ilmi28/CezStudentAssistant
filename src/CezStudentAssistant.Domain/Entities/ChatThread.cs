namespace CezStudentAssistant.Domain.Entities;

public class ChatThread : BaseEntity
{
    public required string Title { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<Resource> AttachedResources { get; set; } = new List<Resource>();
}
