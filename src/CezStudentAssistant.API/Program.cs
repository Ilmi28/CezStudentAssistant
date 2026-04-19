using CezStudentAssistant.API.Endpoints;
using CezStudentAssistant.Application;
using CezStudentAssistant.Domain.Commands;
using CezStudentAssistant.Domain.Exceptions;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Queries;
using CezStudentAssistant.Domain.Responses;
using CezStudentAssistant.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;

namespace CezStudentAssistant.API;

public class Program
{
    public static void Main(string[] args)
    {
        DotNetEnv.Env.Load(".env.local");

        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder);

        var app = builder.Build();

        ConfigureMiddleware(app);
        MapEndpoints(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddApplication();
        builder.Services.AddInstrastructure(builder.Environment.IsDevelopment());
    }

    private static void MapEndpoints(WebApplication app)
    {
        app.MapUserEndpoints();
        app.MapGet(
            "/example-query",
            async ([AsParameters] ExampleQuery query, IQueryHandler<ExampleQuery, ExampleResponse> handler)
            => await handler.HandleAsync(query));

        app.MapPost(
            "/example-command",
            async (ExampleCommand query, ICommandHandler<ExampleCommand, ExampleResponse> handler)
            => await handler.HandleAsync(query));

        app.MapPost(
            "/example-command-without-response",
            async (ExampleCommand query, ICommandHandler<ExampleCommand> handler)
            => await handler.HandleAsync(query));
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        app.UseExceptionHandler(appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionFeature?.Error;

                ApiResponse apiResponse = exception switch
                {
                    AppException appException => appException switch
                    {
                        NotFoundException => new NotFoundResponse(appException.ApiMessage),
                        ConflictException => new ConflictResponse(appException.ApiMessage),
                        ApiValidationException => new ValidationResponse(appException.ApiMessage, ((ApiValidationException)appException).Errors),
                        _ => new ServerErrorResponse()
                    },

                    _ => new ServerErrorResponse()
                };

                context.Response.StatusCode = (int)apiResponse.StatusCode;
                await context.Response.WriteAsJsonAsync((object)apiResponse);
            });
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
    }
}
