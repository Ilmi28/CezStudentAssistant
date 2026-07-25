using CezStudentAssistant.Application.Commands.Course;
using CezStudentAssistant.Application.Commands.Quiz;
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

        group.MapPost("/", async (AddUserCourseCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserCourseCommand command, IMediator mediator) =>
        {
            command.CourseId = id;
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var command = new DeleteUserCourseCommand { CourseId = id };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetCourseDetailsQuery { CourseId = id };
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/{courseId:guid}/file/{fileId:guid}/download", async (Guid courseId, Guid fileId, IMediator mediator) =>
        {
            var query = new DownloadCourseFileQuery { CourseId = courseId, FileId = fileId };
            var result = await mediator.Send(query);
            return Results.File(result.FileStream, result.ContentType, result.FileName);
        }).RequireAuthorization();

        group.MapPost("/{courseId:guid}/file", async (Guid courseId, IFormFile file, IMediator mediator) =>
        {
            using var stream = file.OpenReadStream();
            var command = new UploadCourseFileCommand
            {
                CourseId = courseId,
                FileStream = stream,
                FileName = file.FileName,
                ContentType = file.ContentType
            };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization().DisableAntiforgery();

        group.MapPost("/{courseId:guid}/generate-quiz", async (Guid courseId, GenerateQuizCommand command, IMediator mediator) =>
        {
            command.CourseId = courseId;
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}
