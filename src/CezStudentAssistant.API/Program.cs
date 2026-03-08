using CezStudentAssistant.API.Extensions;
using CezStudentAssistant.Domain.Commands;
using CezStudentAssistant.Domain.DTOs;
using CezStudentAssistant.Domain.Exceptions;
using CezStudentAssistant.Domain.Interfaces.CQRS;
using CezStudentAssistant.Domain.Interfaces.Persistence.Data;
using CezStudentAssistant.Domain.Queries;
using CezStudentAssistant.Domain.Responses;
using CezStudentAssistant.Infrastructure.Persistence.Data;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

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
        builder.Services.AddServices();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddHttpContextAccessor();

        if (builder.Environment.IsDevelopment())
            builder.Services.AddSqlServer(loggingEnabled: true, detailedErrors: true);
        else
            builder.Services.AddSqlServer();
    }

    private static void MapEndpoints(WebApplication app)
    {
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

                if (exceptionFeature?.Error is AppException appException)
                {
                    ApiResponse apiResponse = appException switch
                    {
                        NotFoundException => new NotFoundResponse(appException.ApiMessage),
                        _ => new ServerErrorResponse(appException.ApiMessage)
                    };

                    context.Response.StatusCode = (int)apiResponse.StatusCode;
                    await context.Response.WriteAsJsonAsync(apiResponse);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsJsonAsync(new ServerErrorResponse(CommonApiMessage.AppServerError));
                }
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
