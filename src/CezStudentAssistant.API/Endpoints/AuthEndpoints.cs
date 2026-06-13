using CezStudentAssistant.API.Requests.Auth;
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
            async (RegisterUserRequest request, IMediator mediator) =>
            {
                var response = await mediator.Send(new RegisterUserCommand(request.UserName, request.Password));

                return Results.Ok(response);
            });

        group.MapPost(
            "/login",
            async (LoginUserRequest request, IMediator mediator) =>
            {
                var response = await mediator.Send(new LoginUserCommand(request.UserName, request.Password));

                return Results.Ok(response);
            });

        group.MapPost(
            "/login-cez",
            async (LoginWithCezRequest request, IMediator mediator) =>
            {
                var response = await mediator.Send(new LoginWithCezCommand(request.UserName, request.Password));

                return Results.Ok(response);
            });
    }
}
