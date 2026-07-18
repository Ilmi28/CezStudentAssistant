namespace CezStudentAssistant.Domain.Entities;

public class SelectedQuizOption : BaseEntity
{
    public Guid QuestionAnswerId { get; set; }
    public Guid QuestionOptionId { get; set; }

    public QuestionAnswer QuestionAnswer { get; set; } = null!;
    public QuestionOption QuestionOption { get; set; } = null!;
}
