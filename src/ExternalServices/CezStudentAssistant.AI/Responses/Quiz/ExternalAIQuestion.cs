using System.Collections.Generic;

namespace CezStudentAssistant.AI.Responses.Quiz;

public class ExternalAIQuestion
{
    public required string Content { get; set; }
    public required string QuestionType { get; set; }
    public decimal Points { get; set; }
    public ICollection<ExternalAIQuestionOption> Options { get; set; } = new List<ExternalAIQuestionOption>();
}
