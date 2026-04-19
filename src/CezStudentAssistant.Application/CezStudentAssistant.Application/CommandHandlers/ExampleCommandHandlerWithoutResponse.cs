using CezStudentAssistant.Domain.Commands;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Responses;
using System.Net;

namespace CezStudentAssistant.Domain.CommandHandlers;

public class ExampleCommandHandlerWithoutResponse : ICommandHandler<ExampleCommand>
{
    public async Task<ApiResponse> HandleAsync(ExampleCommand command, CancellationToken cancellationToken = default)
    {
        await Task.Delay(3000, cancellationToken);
        return new ApiResponse
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Example command executed successfully.",
        };
    }
}
