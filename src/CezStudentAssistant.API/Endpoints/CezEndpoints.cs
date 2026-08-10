using CezStudentAssistant.Application.Commands.Cez;
using CezStudentAssistant.Application.Queries.Cez;
using MediatR;

namespace CezStudentAssistant.API.Endpoints;

public static class CezEndpoints
{
    public static void MapCezEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cez").WithTags("CEZ");

        group.MapGet("/status", async (IMediator mediator) =>
        {
            var query = new GetCezStatusQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).WithName("GetCezStatus").RequireAuthorization();

        group.MapPost("/sync-courses", async (IMediator mediator) =>
        {
            var command = new SyncCezCoursesCommand();
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).WithName("SyncCezCourses").RequireAuthorization();
    }
}
