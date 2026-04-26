using CezStudentAssistant.API.Endpoints;
using CezStudentAssistant.Application;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Cez;
using CezStudentAssistant.Domain.Exceptions;
using CezStudentAssistant.Domain.Responses;
using CezStudentAssistant.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;

namespace CezStudentAssistant.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if (builder.Environment.IsDevelopment())
            DotNetEnv.Env.Load();

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
        builder.Services.AddCez();
    }

    private static void MapEndpoints(WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapUserEndpoints();
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
                        BadGatewayException => new BadGatewayResponse(appException.ApiMessage),
                        UnauthorizedException => new UnauthorizedResponse(appException.ApiMessage),
                        BadRequestException => new BadRequestResponse(appException.ApiMessage),
                        _ => new ServerErrorResponse(appException.ApiMessage)
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
