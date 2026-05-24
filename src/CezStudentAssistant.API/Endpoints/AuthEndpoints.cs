using CezStudentAssistant.API.Requests.Auth;
using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Responses.Cez;

namespace CezStudentAssistant.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost(
            "/register",
            async (RegisterUserRequest request, ICommandHandler<RegisterUserCommand, Guid> handler)
            => await handler.HandleAsync(new RegisterUserCommand
            {
                UserName = request.UserName,
                Password = request.Password
            }));

        group.MapPost(
            "/login",
            async (LoginUserRequest request, ICommandHandler<LoginUserCommand, Guid> handler)
            => await handler.HandleAsync(new LoginUserCommand
            {
                UserName = request.UserName,
                Password = request.Password
            }));

        group.MapPost(
            "/login-cez",
            async (LoginWithCezRequest request, ICommandHandler<LoginWithCezCommand, CezLoginResponse> handler)
            => await handler.HandleAsync(new LoginWithCezCommand
            {
                UserName = request.UserName,
                Password = request.Password
            }));
    }
}
