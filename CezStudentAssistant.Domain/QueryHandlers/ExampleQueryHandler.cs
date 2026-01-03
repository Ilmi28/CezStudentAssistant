using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Queries;
using CezStudentAssistant.Domain.Responses;
using System.Net;

namespace CezStudentAssistant.Domain.QueryHandlers;

public class ExampleQueryHandler : IQueryHandler<ExampleQuery, ExampleResponse>
{
    public async Task<ApiResponse<ExampleResponse>> HandleAsync(ExampleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(3000, cancellationToken);
        return new ApiResponse<ExampleResponse>
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Example command executed successfully."
        };
    }
}