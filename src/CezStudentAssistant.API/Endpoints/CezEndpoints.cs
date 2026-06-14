using CezStudentAssistant.Application.Commands.Cez;
using MediatR;

namespace CezStudentAssistant.API.Endpoints;

public static class CezEndpoints
{
    public static void MapCezEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/cez").WithTags("CEZ");

        group.MapPost("/sync-courses", async (IMediator mediator) =>
        {
            var command = new SyncCezCoursesCommand();
            var result = await mediator.Send(command);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        }).WithName("SyncCezCourses").RequireAuthorization();
    }
}
