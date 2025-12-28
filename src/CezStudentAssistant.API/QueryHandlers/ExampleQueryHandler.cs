using CezStudentAssistant.API.Interfaces.CQRS;
using CezStudentAssistant.API.Queries;
using CezStudentAssistant.API.Responses;
using System.Net;

namespace CezStudentAssistant.API.QueryHandlers
{
    public class ExampleQueryHandler : IQueryHandler<ExampleQuery, ExampleResponse>
    {
        public async Task<ApiResponse<ExampleResponse>> HandleAsync(ExampleQuery query, CancellationToken cancellationToken = default)
        {
            await Task.Delay(3000, cancellationToken);
            return new ApiResponse<ExampleResponse>
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Example command executed successfully.",
            };
        }
    }
}
