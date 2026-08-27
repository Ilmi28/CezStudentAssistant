using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class Quiz : BaseEntity
{
    public required string Name { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public QuizStatusEnum Status { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int? QuestionCountPerAttempt { get; set; }
    public int? EasyQuestionCountPerAttempt { get; set; }
    public int? MediumQuestionCountPerAttempt { get; set; }
    public int? HardQuestionCountPerAttempt { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
