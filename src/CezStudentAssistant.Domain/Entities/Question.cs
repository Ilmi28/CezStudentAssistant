using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Domain.Entities;

public class Question : BaseEntity
{
    public required string Content { get; set; }
    public QuestionType Type { get; set; }
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    public Guid CourseId { get; set; }
    public required Course Course { get; set; }
}
