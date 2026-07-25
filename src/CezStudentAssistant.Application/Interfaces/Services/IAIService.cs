using CezStudentAssistant.Application.Dtos.AI;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface IAIService
{
    Task GenerateQuiz(GenerateQuizDto dto, CancellationToken ct = default);
}
