using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses.AI.Quiz;
using CezStudentAssistant.Application.Responses.AI.Flashcard;

namespace CezStudentAssistant.Application.Interfaces.External;

public interface IAIClient
{
    Task<AIQuizResponse> GenerateQuizAsync(AIQuizRequest request);
    Task<AIFlashcardDeck> GenerateFlashcardsAsync(AIFlashcardRequest request);
    Task<int> EstimateTokenUsageAsync(AIQuizRequest request);
    Task<int> EstimateTextTokenUsageAsync(string text);
    IAsyncEnumerable<string> StreamChatResponseAsync(
        IEnumerable<CezStudentAssistant.Domain.Entities.ChatMessage> history,
        string userPrompt,
        IEnumerable<AIFile>? files = null,
        string? courseName = null,
        CancellationToken ct = default);
    Task<string> GenerateChatTitleAsync(string userMessage, string assistantResponse, CancellationToken ct = default);
}
