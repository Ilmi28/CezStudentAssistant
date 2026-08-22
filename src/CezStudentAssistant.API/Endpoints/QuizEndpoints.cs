using CezStudentAssistant.Application.Commands.Quiz;
using CezStudentAssistant.Application.Queries.Quiz;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace CezStudentAssistant.API.Endpoints;

public static class QuizEndpoints
{
    public static void MapQuizEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/quiz").WithTags("Quiz");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var query = new GetUserQuizzesQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetQuizByIdQuery { QuizId = id };
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/{id:guid}/start", async (Guid id, IMediator mediator) =>
        {
            var command = new StartQuizCommand { QuizId = id };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/attempt/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetQuizAttemptByIdQuery { QuizAttemptId = id };
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/attempt/{id:guid}/complete", async (Guid id, IMediator mediator) =>
        {
            var command = new CompleteQuizAttemptCommand { QuizAttemptId = id };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/answer", async (SubmitQuizAnswerCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateQuizCommand command, IMediator mediator) =>
        {
            command.QuizId = id;
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}
