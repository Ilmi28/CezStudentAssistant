using CezStudentAssistant.Application.Responses.Cez;

namespace CezStudentAssistant.Domain.Interfaces.Services;

public interface ICezAuthService
{
    Task<CezLoginResponse> LoginWithCezAsync(
        string userName,
        string password,
        object messageSource,
        CancellationToken ct = default
    );
}
