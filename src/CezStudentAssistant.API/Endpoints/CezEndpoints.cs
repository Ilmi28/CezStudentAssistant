using CezStudentAssistant.Application.Commands.Cez;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CezStudentAssistant.API.Endpoints;

public static class CezEndpoints
{
    public static void MapCezEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/cez").WithTags("CEZ");

        group.MapPost("/sync-courses", async ([FromQuery] Guid userId, IMediator mediator) =>
        {
            var command = new SyncCezCoursesCommand(userId);
            var result = await mediator.Send(command);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        }).WithName("SyncCezCourses");
    }
}
