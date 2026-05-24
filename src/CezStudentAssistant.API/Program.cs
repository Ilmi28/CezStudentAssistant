using System.Text;
using CezStudentAssistant.API.Endpoints;
using CezStudentAssistant.Application;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Cez;
using CezStudentAssistant.Infrastructure;
using CezStudentAssistant.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;

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
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings?.Issuer,
                ValidAudience = jwtSettings?.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? string.Empty))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies["accessToken"];
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization();

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

        app.UseAuthentication();
        app.UseAuthorization();
    }
}
