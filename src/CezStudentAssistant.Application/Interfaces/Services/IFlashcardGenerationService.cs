using CezStudentAssistant.Application.Dtos.AI;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface IFlashcardGenerationService
{
    Task GenerateFlashcards(GenerateFlashcardsDto dto, CancellationToken ct = default);
}
