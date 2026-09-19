using CezStudentAssistant.Application.Commands.Chat;
using CezStudentAssistant.Application.Queries.Chat;
using CezStudentAssistant.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text.Json;

namespace CezStudentAssistant.API.Endpoints;

public static class ChatEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/chats").WithTags("Chat");

        group.MapGet("/", async (
            Guid? courseId,
            int? pageNumber,
            int? pageSize,
            string? searchTerm,
            IMediator mediator) =>
        {
            var query = new GetUserCourseChatThreadsQuery
            {
                CourseId = courseId,
                PageNumber = pageNumber ?? 1,
                PageSize = pageSize ?? 10,
                SearchTerm = searchTerm
            };
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/", async (CreateChatThreadCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetChatThreadByIdQuery { ChatThreadId = id };
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/{id:guid}/messages", async (Guid id, IMediator mediator) =>
        {
            var query = new GetChatThreadMessagesQuery { ChatThreadId = id };
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPut("/{id:guid}/resources", async (Guid id, UpdateChatThreadResourcesRequest request, IMediator mediator) =>
        {
            var command = new UpdateChatThreadResourcesCommand
            {
                ChatThreadId = id,
                ResourceIds = request.ResourceIds
            };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/{id:guid}/stream", async (
            Guid id,
            SendChatMessageStreamRequest request,
            ICurrentUserService currentUserService,
            SendChatMessageStreamCommandHandler handler,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userId = currentUserService.GetCurrentUserId()
                ?? throw new CezStudentAssistant.Application.Exceptions.UnauthorizedException(CezStudentAssistant.API.Consts.MessageConsts.NotAuthenticated);
            var command = new SendChatMessageStreamCommand
            {
                UserId = userId,
                ChatThreadId = id,
                UserMessage = request.UserMessage
            };

            httpContext.Response.ContentType = "text/event-stream";
            httpContext.Response.Headers.CacheControl = "no-cache";

            await foreach (var chunk in handler.ExecuteStreamAsync(command, ct))
            {
                var jsonChunk = JsonSerializer.Serialize(new { chunk });
                await httpContext.Response.WriteAsync($"data: {jsonChunk}\n\n", ct);
                await httpContext.Response.Body.FlushAsync(ct);
            }

            await httpContext.Response.WriteAsync("data: [DONE]\n\n", ct);
            await httpContext.Response.Body.FlushAsync(ct);
        }).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var command = new DeleteChatThreadCommand { ChatThreadId = id };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}

public class SendChatMessageStreamRequest
{
    public required string UserMessage { get; set; }
}

public class UpdateChatThreadResourcesRequest
{
    public List<Guid> ResourceIds { get; set; } = [];
}
