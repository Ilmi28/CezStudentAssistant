using CezStudentAssistant.Application.Interfaces.Common;
using CezStudentAssistant.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface ICascadeDeleteService : IScopedService
{
    Task DeleteCourseCascadeAsync(Course course, CancellationToken ct = default);
    Task DeleteQuizCascadeAsync(Quiz quiz, CancellationToken ct = default);
    Task DeleteFlashcardDeckCascadeAsync(FlashcardDeck deck, CancellationToken ct = default);
    Task DeleteChatThreadCascadeAsync(ChatThread thread, CancellationToken ct = default);
}
