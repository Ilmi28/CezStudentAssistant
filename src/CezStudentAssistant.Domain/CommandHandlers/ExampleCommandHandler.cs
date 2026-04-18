using CezStudentAssistant.Domain.Commands;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Domain.CommandHandlers;

public class ExampleCommandHandler : BaseCommandHandler<ExampleCommand, ExampleResponse>,
    ICommandHandler<ExampleCommand, ExampleResponse>
{
    protected override ApiMessage SuccessMessage => new ApiMessage("SUCCESS", "Operation completed successfully");

    protected override ApiMessage ErrorMessage => new ApiMessage("ERROR", "An error occurred");

    protected async override Task<ExampleResponse> ExecuteAsync(ExampleCommand command, CancellationToken ct)
    {
        return new ExampleResponse();
    }
}
