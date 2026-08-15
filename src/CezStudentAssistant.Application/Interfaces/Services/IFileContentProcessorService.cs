using CezStudentAssistant.Application.Dtos.AI;
using CezStudentAssistant.Application.Requests.AI;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface IFileContentProcessorService
{
    Task<ProcessedFileContent?> ProcessFileAsync(AIFile file, CancellationToken cancellationToken = default);
}
