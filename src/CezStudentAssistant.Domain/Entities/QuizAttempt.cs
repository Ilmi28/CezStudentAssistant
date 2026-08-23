using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class QuizAttempt : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid QuizId { get; set; }
    public QuizAttemptStatus Status { get; set; }
    public decimal? Points { get; set; }
    public decimal? MaxPoints { get; set; }
    public int QuestionCount { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
    public Quiz Quiz { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public User User { get; set; } = null!;
}
