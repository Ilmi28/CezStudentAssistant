using CezStudentAssistant.Application.Dtos.Cez;

namespace CezStudentAssistant.Application.Interfaces.Services;

public interface ICezService
{
    Task<CezUserInfo> LoginWithCezAsync(string userName, string password, CancellationToken ct = default);
    Task<Guid> SyncCezUser(CezUserInfo cezUserInfo, CancellationToken ct = default);
}
