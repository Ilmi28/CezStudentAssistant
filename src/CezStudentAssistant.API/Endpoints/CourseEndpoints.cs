using CezStudentAssistant.Application.Queries.Course;
using MediatR;

namespace CezStudentAssistant.API.Endpoints;

public static class CourseEndpoints
{
    public static void MapCourseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/course").WithTags("Course");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var query = new GetUserCoursesQuery();
            var result = await mediator.Send(query);

            return Results.Ok(result);
        }).RequireAuthorization();
    }
}
