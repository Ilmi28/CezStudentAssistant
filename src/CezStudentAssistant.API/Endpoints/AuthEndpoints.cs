using CezStudentAssistant.API.Requests.Auth;
using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Interfaces.CQRS;

namespace CezStudentAssistant.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost(
            "/register",
            async (RegisterUserRequest request, ICommandHandler<RegisterUserCommand> handler) =>
            {
                var response = await handler.HandleAsync(new RegisterUserCommand
                {
                    UserName = request.UserName,
                    Password = request.Password
                });

                return Results.Ok(response);
            });

        group.MapPost(
            "/login",
            async (LoginUserRequest request, ICommandHandler<LoginUserCommand> handler) =>
            {
                var response = await handler.HandleAsync(new LoginUserCommand
                {
                    UserName = request.UserName,
                    Password = request.Password
                });

                return Results.Ok(response);
            });

        group.MapPost(
            "/login-cez",
            async (LoginWithCezRequest request, ICommandHandler<LoginWithCezCommand> handler) =>
            {
                var response = await handler.HandleAsync(new LoginWithCezCommand
                {
                    UserName = request.UserName,
                    Password = request.Password
                });

                return Results.Ok(response);
            });
    }
}
