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
    public static void MapQuizEndpoints(this WebApplication app)
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

        group.MapPost("/answer", async (SubmitQuizAnswerCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/start", async (StartQuizCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}
