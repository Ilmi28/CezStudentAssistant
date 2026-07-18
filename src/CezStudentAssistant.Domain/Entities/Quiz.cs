namespace CezStudentAssistant.Domain.Entities;

public class Quiz : BaseEntity
{
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public Guid CourseId { get; set; }

    public Course Course { get; set; } = null!;
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
