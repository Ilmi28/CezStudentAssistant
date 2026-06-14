using CezStudentAssistant.Application.Commands.Auth;
using MediatR;

namespace CezStudentAssistant.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost(
            "/register",
            async (RegisterUserCommand command, IMediator mediator) =>
            {
                var response = await mediator.Send(command);

                return Results.Ok(response);
            });

        group.MapPost(
            "/login",
            async (LoginUserCommand command, IMediator mediator) =>
            {
                var response = await mediator.Send(command);

                return Results.Ok(response);
            });

        group.MapPost(
            "/login-cez",
            async (LoginWithCezCommand command, IMediator mediator) =>
            {
                var response = await mediator.Send(command);

                return Results.Ok(response);
            });
    }
}
