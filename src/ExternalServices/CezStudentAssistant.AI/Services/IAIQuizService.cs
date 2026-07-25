using Google.GenAI.Types;
using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses.AI.Quiz;

namespace CezStudentAssistant.AI.Services;

public interface IAIQuizService
{
    string BuildPrompt(AIQuizRequest request);
    Schema BuildSchema();
    AIQuizResponse ParseResponse(string jsonText);
}
