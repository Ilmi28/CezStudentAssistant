using CezStudentAssistant.Application.Commands.Course;
using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Queries.Course;
using CezStudentAssistant.Application.Queries.Quiz;
using MediatR;

namespace CezStudentAssistant.API.Endpoints;

public static class CourseEndpoints
{
    public static void MapCourseEndpoints(this IEndpointRouteBuilder app)
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

        group.MapPut("/", async (UpdateUserCourseCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapDelete("/{courseId:guid}", async (Guid courseId, IMediator mediator) =>
        {
            var command = new DeleteUserCourseCommand { CourseId = courseId };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetCourseDetailsQuery { CourseId = id };
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/file/upload", async (IFormFile file, Guid courseId, IMediator mediator) =>
        {
            using var stream = file.OpenReadStream();
            var command = new UploadCourseFileCommand
            {
                CourseId = courseId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileStream = stream
            };

            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization().DisableAntiforgery();

        group.MapGet("/{courseId:guid}/file/{fileId:guid}/download", async (Guid courseId, Guid fileId, IMediator mediator) =>
        {
            var query = new DownloadCourseFileQuery { CourseId = courseId, FileId = fileId };
            var result = await mediator.Send(query);
            return Results.File(result.FileStream, result.ContentType, result.FileName);
        }).RequireAuthorization();

        group.MapDelete("/{courseId:guid}/file/{fileId:guid}", async (Guid courseId, Guid fileId, IMediator mediator) =>
        {
            var command = new DeleteCourseFileCommand { CourseId = courseId, FileId = fileId };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/{courseId:guid}/estimate-quiz-tokens", async (Guid courseId, EstimateQuizTokensQuery query, IMediator mediator) =>
        {
            query.CourseId = courseId;
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/{courseId:guid}/generate-quiz", async (Guid courseId, GenerateQuizCommand command, IMediator mediator) =>
        {
            command.CourseId = courseId;
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}
