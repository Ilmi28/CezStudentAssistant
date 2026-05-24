using CezStudentAssistant.API.Requests.Auth;
using CezStudentAssistant.Application.Commands;
using CezStudentAssistant.Application.Dtos.Auth;
using CezStudentAssistant.Application.Interfaces.CQRS;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Application.Responses.Cez;

namespace CezStudentAssistant.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost(
            "/register",
            async (RegisterUserRequest request, ICommandHandler<RegisterUserCommand, Guid> handler, IJwtService jwtService, HttpContext context) =>
            {
                var response = await handler.HandleAsync(new RegisterUserCommand
                {
                    UserName = request.UserName,
                    Password = request.Password
                });

                var tokens = await jwtService.GenerateTokensAsync(response.Data, request.UserName);
                SetAuthCookies(context.Response, tokens.AccessToken, tokens.RefreshToken);

                return Results.Ok(response);
            });

        group.MapPost(
            "/login",
            async (LoginUserRequest request, ICommandHandler<LoginUserCommand, Guid> handler, IJwtService jwtService, HttpContext context) =>
            {
                var response = await handler.HandleAsync(new LoginUserCommand
                {
                    UserName = request.UserName,
                    Password = request.Password
                });

                var tokens = await jwtService.GenerateTokensAsync(response.Data, request.UserName);
                SetAuthCookies(context.Response, tokens.AccessToken, tokens.RefreshToken);

                return Results.Ok(response);
            });

        group.MapPost(
            "/login-cez",
            async (LoginWithCezRequest request, ICommandHandler<LoginWithCezCommand, CezLoginResponse> handler, IJwtService jwtService, HttpContext context) =>
            {
                var response = await handler.HandleAsync(new LoginWithCezCommand
                {
                    UserName = request.UserName,
                    Password = request.Password
                });

                if (response.Data != null)
                {
                    var tokens = await jwtService.GenerateTokensAsync(response.Data.UserId, request.UserName);
                    SetAuthCookies(context.Response, tokens.AccessToken, tokens.RefreshToken);
                }

                return Results.Ok(response);
            });

        group.MapPost(
            "/refresh",
            async (IJwtService jwtService, HttpContext context) =>
            {
                var refreshToken = context.Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Results.Unauthorized();
                }

                var tokens = await jwtService.RefreshTokensAsync(refreshToken);
                SetAuthCookies(context.Response, tokens.AccessToken, tokens.RefreshToken);

                return Results.Ok(new SuccessResponse(new ApiMessage(null, "Token refreshed successfully.")));
            });

        group.MapPost(
            "/logout",
            async (IJwtService jwtService, HttpContext context) =>
            {
                var refreshToken = context.Request.Cookies["refreshToken"];
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await jwtService.RevokeTokenAsync(refreshToken);
                }

                ClearAuthCookies(context.Response);

                return Results.Ok(new SuccessResponse(new ApiMessage(null, "Logged out successfully.")));
            });
    }

    private static void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        response.Cookies.Append("accessToken", accessToken, cookieOptions);
        response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private static void ClearAuthCookies(HttpResponse response)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        response.Cookies.Delete("accessToken", cookieOptions);
        response.Cookies.Delete("refreshToken", cookieOptions);
    }
}
