using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class Question : BaseEntity
{
    public required string Content { get; set; }
    public QuestionType Type { get; set; }
    public Guid QuizId { get; set; }
    public decimal Points { get; set; }

    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    public Quiz Quiz { get; set; } = null!;
}
