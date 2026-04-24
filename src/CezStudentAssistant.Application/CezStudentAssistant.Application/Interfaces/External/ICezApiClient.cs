using CezStudentAssistant.Application.Requests.Cez;
using CezStudentAssistant.Application.Responses.Cez;

namespace CezStudentAssistant.Application.Interfaces.External;

public interface ICezApiClient
{
    Task<CezLoginResponse> LoginToCez(CezLoginRequest loginDto);
}
