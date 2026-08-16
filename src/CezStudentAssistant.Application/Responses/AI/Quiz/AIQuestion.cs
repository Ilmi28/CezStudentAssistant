using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Application.Responses.AI.Quiz;

public class AIQuestion
{
    public required string Content { get; set; }
    public QuestionType QuestionType { get; set; }
    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.Medium;
    public ICollection<AIQuestionOption> Options { get; set; } = new List<AIQuestionOption>();
}
