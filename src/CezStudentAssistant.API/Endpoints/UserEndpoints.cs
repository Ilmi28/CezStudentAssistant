using CezStudentAssistant.Application.Commands.UserConfiguration;
using CezStudentAssistant.Application.Queries.User;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CezStudentAssistant.API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user").WithTags("User");

        group.MapGet("/configuration", async (IMediator mediator) =>
        {
            var query = new GetUserConfigurationQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPut("/configuration", async (UpdateUserConfigurationCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/usage", async (IMediator mediator) =>
        {
            var query = new GetUserUsageQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/dashboard-stats", async (IMediator mediator) =>
        {
            var query = new GetDashboardStatsQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapGet("/recent-activity", async (int? limit, IMediator mediator) =>
        {
            var query = new GetRecentActivityQuery { Limit = limit ?? 6 };
            var result = await mediator.Send(query);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}


