using CezStudentAssistant.API.Extensions;
using CezStudentAssistant.Domain.Commands;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Queries;
using CezStudentAssistant.Domain.Responses;
using FluentValidation;

namespace CezStudentAssistant.API;

public class Program
{
    public static void Main(string[] args)
    {
        DotNetEnv.Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder);

        var app = builder.Build();

        MapEndpoints(app);
        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddCqrsHandlers();
        builder.Services.AddLoggingDecorator();
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        builder.Services.AddRepositories();

        if (builder.Environment.IsDevelopment())
            builder.Services.AddSqlServer(loggingEnabled: true, detailedErrors: true);
        else
            builder.Services.AddSqlServer();
    }

    private static void MapEndpoints(WebApplication app)
    {
        app.MapGet(
            "/example-query",
            async (
                [AsParameters] ExampleQuery query,
                IQueryHandler<ExampleQuery, ExampleResponse> handler
            ) => await handler.HandleAsync(query)
        );

        app.MapPost(
            "/example-command",
            async (
                ExampleCommand query,
                ICommandHandler<ExampleCommand, ExampleResponse> handler
            ) => await handler.HandleAsync(query)
        );

        app.MapPost(
            "/example-command-without-response",
            async (ExampleCommand query, ICommandHandler<ExampleCommand> handler) =>
                await handler.HandleAsync(query)
        );
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
    }
}
