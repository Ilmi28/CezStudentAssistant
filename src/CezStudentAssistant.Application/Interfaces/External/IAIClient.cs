using CezStudentAssistant.Application.Requests.AI;
using CezStudentAssistant.Application.Responses.AI.Quiz;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Interfaces.External;

public interface IAIClient
{
    Task<AIQuizResponse> GenerateQuizAsync(AIQuizRequest request);
}
