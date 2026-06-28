using CezStudentAssistant.Application.Requests.Cez;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface ICezService
{
    Task<Guid> LoginWithCezAsync(string userName, string password, CancellationToken ct = default);
    Task SyncUserCourses(Guid userId, CancellationToken ct = default);
    Task SyncCourseContent(CezCourseRequest courseRequest, CancellationToken cancellationToken);
}
