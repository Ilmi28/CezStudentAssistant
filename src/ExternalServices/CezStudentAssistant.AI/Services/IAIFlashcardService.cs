using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses.AI.Flashcard;
using Google.GenAI.Types;

namespace CezStudentAssistant.AI.Services;

public interface IAIFlashcardService
{
    string BuildPrompt(AIFlashcardRequest request);
    Schema BuildSchema();
    AIFlashcardDeck ParseResponse(string jsonText);
}
