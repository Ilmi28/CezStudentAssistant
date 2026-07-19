namespace CezStudentAssistant.AI.Responses.Quiz;

public class ExternalAIQuiz
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required List<ExternalAIQuestion> Questions { get; set; } = new List<ExternalAIQuestion>();
}
