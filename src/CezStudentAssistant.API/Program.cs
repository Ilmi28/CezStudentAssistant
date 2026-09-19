
using CezStudentAssistant.AI;
using CezStudentAssistant.API.Consts;
using CezStudentAssistant.API.Endpoints;
using CezStudentAssistant.API.Hubs;
using CezStudentAssistant.API.Services;
using CezStudentAssistant.Application;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Cez;
using CezStudentAssistant.Infrastructure;
using CezStudentAssistant.Infrastructure.Settings;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CezStudentAssistant.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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
            var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>().Validate();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/sync-hub"))
                    {
                        context.Token = accessToken;
                    }
                    else
                    {
                        context.Token = context.Request.Cookies["accessToken"];
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var response = new UnauthorizedResponse(MessageConsts.NotAuthenticated);

                    await context.Response.WriteAsJsonAsync(response);
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var response = new ForbiddenResponse(MessageConsts.Forbidden);

                    await context.Response.WriteAsJsonAsync(response);
                }
            };
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddApplication();
        builder.Services.AddInstrastructure(builder.Configuration, builder.Environment.IsDevelopment());
        builder.Services.AddCez(builder.Configuration);
        builder.Services.AddAI(builder.Configuration);
        builder.Services.AddSignalR();

        builder.Services.AddScoped<IJobNotificationService, SignalRJobNotificationService>();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(connectionString);
            }));
        builder.Services.AddHangfireServer();
    }

    private static void MapEndpoints(WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapUserEndpoints();
        app.MapCezEndpoints();
        app.MapCourseEndpoints();
        app.MapQuizEndpoints();
        app.MapFlashcardEndpoints();
        app.MapChatEndpoints();
        app.MapHub<CezSyncNotificationHub>("/sync-hub");
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        app.UseExceptionHandler(appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionFeature?.Error;

                ApiResponse apiResponse = exception switch
                {
                    AppException appException => appException switch
                    {
                        NotFoundException => new NotFoundResponse(appException.Message),
                        ConflictException => new ConflictResponse(appException.Message),
                        ApiValidationException validationException => new ValidationResponse(validationException.Message, validationException.Errors),
                        BadGatewayException => new BadGatewayResponse(appException.Message),
                        UnauthorizedException => new UnauthorizedResponse(appException.Message),
                        ForbiddenException => new ForbiddenResponse(appException.Message),
                        BadRequestException => new BadRequestResponse(appException.Message),
                        _ => new ServerErrorResponse(appException.Message)
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

        app.UseCors("CorsPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new AllowAllDashboardAuthorizationFilter()]
        });
    }
}

sealed class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
