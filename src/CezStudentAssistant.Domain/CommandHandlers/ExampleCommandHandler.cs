using System.Net;
using CezStudentAssistant.Domain.Commands;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Interfaces.Persistence.Data;
using CezStudentAssistant.Domain.Interfaces.Persistence.Repositories;
using CezStudentAssistant.Domain.Responses;

namespace CezStudentAssistant.Domain.CommandHandlers;

public class ExampleCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<ExampleCommand, ExampleResponse>
{
    public async Task<ApiResponse<ExampleResponse>> HandleAsync(
        ExampleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        await unitOfWork
            .Repository<IExampleEntityRepository>()
            .AddAsync(new Entities.ExampleEntity());
        return new ApiResponse<ExampleResponse>
        {
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Example command executed successfully.",
        };
    }
}
