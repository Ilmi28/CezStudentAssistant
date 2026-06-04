namespace CezStudentAssistant.Domain.Entities;

public class QuizAttempt : BaseEntity
{
    public Guid UserId { get; set; }
    public required User User { get; set; }
    public Guid CourseId { get; set; }
    public required Course Course { get; set; }
    public decimal Score { get; set; }
    public bool IsCompleted { get; set; }
    public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
}
