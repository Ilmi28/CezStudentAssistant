using CezStudentAssistant.Application.Interfaces.External;
using CezStudentAssistant.Application.Responses.AI.Quiz;

namespace CezStudentAssistant.AI;

public class GeminiAIClient : IAIClient
{
    public Task<AIQuizResponse> GenerateQuizAsync(IEnumerable<Stream> files)
    {
        throw new NotImplementedException();
    }
}
