using System.Collections.Generic;

namespace CezStudentAssistant.Application.Responses.AI.Flashcard;

public class AIFlashcardDeck
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public List<AIFlashcard> Cards { get; set; } = new();
}
