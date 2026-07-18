namespace CezStudentAssistant.Application.Responses.AI.Quiz;

public class AIQuiz
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required List<AIQuestion> Questions { get; set; } = new List<AIQuestion>();
}
