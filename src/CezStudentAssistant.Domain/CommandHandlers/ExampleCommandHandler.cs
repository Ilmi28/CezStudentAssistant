using CezStudentAssistant.Domain.Commands;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Responses;
using System.Net;

namespace CezStudentAssistant.Domain.CommandHandlers;

public class ExampleCommandHandler : ICommandHandler<ExampleCommand, ExampleResponse>
{
    public async Task<ApiResponse<ExampleResponse>> HandleAsync(
        ExampleCommand command,
        CancellationToken cancellationToken = default
    )
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
