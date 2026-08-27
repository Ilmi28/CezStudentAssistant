using CezStudentAssistant.Domain.Enums;

namespace CezStudentAssistant.Application.Responses.AI.Flashcard;

public class AIFlashcard
{
    public required string Front { get; set; }
    public required string Back { get; set; }
    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.Medium;
}
