using CezStudentAssistant.API.Commands;
using CezStudentAssistant.API.Interfaces.CQRS;
using CezStudentAssistant.API.Responses;

namespace CezStudentAssistant.API.CommandHandlers
{
    public class ExampleCommandHandler : ICommandHandler<ExampleCommand, ExampleResponse>
    {
        public async Task<ExampleResponse> HandleAsync(ExampleCommand command, CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken);
            return new ExampleResponse();
        }
    }
}
