using CezStudentAssistant.Cez.Services;

namespace CezStudentAssistant.Cez.Interfaces;

internal interface ICezRequestService
{
    Task<CezRequestResult<TData>> SendGetAsync<TData>(
        string path,
        IEnumerable<KeyValuePair<string, string>> queryParams
    )
        where TData : class;
}
