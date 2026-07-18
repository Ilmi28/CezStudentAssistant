using CezStudentAssistant.Application.Responses.AI.Quiz;

namespace CezStudentAssistant.Application.Interfaces.External;

public interface IAIClient
{
    Task<AIQuizResponse> GenerateQuizAsync(IEnumerable<Stream> files);
}
