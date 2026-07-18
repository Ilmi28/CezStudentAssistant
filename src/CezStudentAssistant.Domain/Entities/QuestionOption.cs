namespace CezStudentAssistant.Domain.Entities;

public class QuestionOption : BaseEntity
{
    public required string Content { get; set; }
    public bool IsCorrect { get; set; }
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}
