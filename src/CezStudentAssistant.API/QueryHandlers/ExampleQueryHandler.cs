using CezStudentAssistant.API.Interfaces.CQRS;
using CezStudentAssistant.API.Queries;
using CezStudentAssistant.API.Responses;

namespace CezStudentAssistant.API.QueryHandlers
{
    public class ExampleQueryHandler : IQueryHandler<ExampleQuery, ExampleResponse>
    {
        public async Task<ExampleResponse> HandleAsync(ExampleQuery query, CancellationToken cancellationToken = default)
        {
            await Task.Delay(5000,  cancellationToken);
            return new ExampleResponse();
        }
    }
}
