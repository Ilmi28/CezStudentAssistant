namespace CezStudentAssistant.Domain.Entities;

public class QuestionAnswer : BaseEntity
{
    public Guid QuizAttemptId { get; set; }
    public required QuizAttempt QuizAttempt { get; set; }
    public Guid QuestionId { get; set; }
    public required Question Question { get; set; }
    public required string Answer { get; set; }
}
