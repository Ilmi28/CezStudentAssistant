using CezStudentAssistant.Application.Commands.Flashcard;
using CezStudentAssistant.Application.Queries.Flashcard;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace CezStudentAssistant.API.Endpoints;

public static class FlashcardEndpoints
{
    public static void MapFlashcardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/flashcards").WithTags("Flashcards");

        group.MapGet("/", async (Guid? courseId, IMediator mediator) =>
        {
            var query = new GetUserFlashcardDecksQuery { CourseId = courseId };
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetFlashcardDeckByIdQuery { DeckId = id };
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/generate", async (GenerateFlashcardsCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/course/{courseId:guid}/estimate-tokens", async (Guid courseId, EstimateFlashcardTokensQuery query, IMediator mediator) =>
        {
            query.CourseId = courseId;
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateFlashcardDeckCommand command, IMediator mediator) =>
        {
            command.DeckId = id;
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var command = new DeleteFlashcardDeckCommand { DeckId = id };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPut("/card/{cardId:guid}/state", async (Guid cardId, UpdateFlashcardStateCommand command, IMediator mediator) =>
        {
            command.CardId = cardId;
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/{id:guid}/reset", async (Guid id, IMediator mediator) =>
        {
            var command = new ResetFlashcardDeckProgressCommand { DeckId = id };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/{id:guid}/attempt", async (Guid id, StartFlashcardAttemptCommand command, IMediator mediator) =>
        {
            command.DeckId = id;
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/attempt/{attemptId:guid}/card/{cardId:guid}/state", async (Guid attemptId, Guid cardId, SubmitFlashcardAttemptCardStateCommand command, IMediator mediator) =>
        {
            command.AttemptId = attemptId;
            command.CardId = cardId;
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/attempt/{attemptId:guid}/complete", async (Guid attemptId, IMediator mediator) =>
        {
            var command = new CompleteFlashcardAttemptCommand { AttemptId = attemptId };
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}
