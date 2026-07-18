namespace CezStudentAssistant.Domain.Entities;

public class QuestionAnswer : BaseEntity
{
    public Guid QuizAttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public decimal EarnedPoints { get; set; }

    public Question Question { get; set; } = null!;
    public QuizAttempt QuizAttempt { get; set; } = null!;
    public ICollection<SelectedQuizOption> SelectedOptions { get; set; } = new List<SelectedQuizOption>();
}
