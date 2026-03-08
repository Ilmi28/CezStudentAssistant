using CezStudentAssistant.Domain.Commands;
using CezStudentAssistant.Domain.Exceptions;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Interfaces.Persistence.Data;
using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Domain.CommandHandlers;

public class ExampleCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<ExampleCommand, ExampleResponse>
{
    public async Task<ApiResponse<ExampleResponse>> HandleAsync(ExampleCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotFoundException(new ApiMessage("SOMETHING_WRONG", "Something went wrong"));
    }
}
