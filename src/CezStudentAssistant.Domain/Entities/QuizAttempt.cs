namespace CezStudentAssistant.Domain.Entities;

public class QuizAttempt : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
    public decimal Score { get; set; }
    public bool IsCompleted { get; set; }

    public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
    public Quiz Quiz { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public User User { get; set; } = null!;
}
