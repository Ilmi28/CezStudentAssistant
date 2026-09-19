using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ChatThreadId { get; set; }
    public ChatMessageRole Role { get; set; }
    public required string Content { get; set; }
    public int TokenCount { get; set; }

    public ChatThread ChatThread { get; set; } = null!;
}
