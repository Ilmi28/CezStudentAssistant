using CezStudentAssistant.API.Consts;
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
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var message = new ApiMessage(null, MessageConsts.NotAuthenticated);
                    var response = new UnauthorizedResponse(message);

                    await context.Response.WriteAsJsonAsync(response);
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var message = new ApiMessage(null, MessageConsts.Forbidden);
                    var response = new ForbiddenResponse(message);

                    await context.Response.WriteAsJsonAsync(response);
                }
            };
        });

        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddApplication();
        builder.Services.AddInstrastructure(builder.Configuration, builder.Environment.IsDevelopment());
        builder.Services.AddCez(builder.Configuration);
    }

    private static void MapEndpoints(WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapUserEndpoints();
        app.MapCezEndpoints();
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
