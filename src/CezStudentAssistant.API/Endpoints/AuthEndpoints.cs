using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Responses.Cez;
using CezStudentAssistant.Domain.Commands;
using CezStudentAssistant.Domain.Interfaces.CQRS;

namespace CezStudentAssistant.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost(
            "/register",
            async (RegisterUserCommand command, ICommandHandler<RegisterUserCommand, Guid> handler)
            => await handler.HandleAsync(command));

        group.MapPost(
            "/login-cez",
            async (LoginWithCezCommand command, ICommandHandler<LoginWithCezCommand, CezLoginResponse> handler)
            => await handler.HandleAsync(command));
    }
}
