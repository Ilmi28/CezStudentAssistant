using CezStudentAssistant.Application.Dtos.Auth;
using CezStudentAssistant.Application.Exceptions;
using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Application.Responses;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Common;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CezStudentAssistant.Infrastructure.Services;

public class JwtService : IJwtService, ISingletonService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IServiceProvider _serviceProvider;

    public JwtService(IOptions<JwtSettings> jwtOptions, IServiceProvider serviceProvider)
    {
        _jwtSettings = jwtOptions.Value;
        _serviceProvider = serviceProvider;
    }

    public async Task<AuthTokens> GenerateTokensAsync(Guid userId, string userName, CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var refreshTokenRepo = unitOfWork.Repository<IRefreshTokenRepository>();

        var accessToken = GenerateAccessToken(userId, userName);
        var refreshTokenValue = GenerateRefreshTokenValue();
        var expiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

        var existingToken = await refreshTokenRepo.GetSingleAsync(x => x.UserId == userId, ct);

        if (existingToken != null)
        {
            existingToken.Token = refreshTokenValue;
            existingToken.ExpiryTime = expiryTime;
            await refreshTokenRepo.UpdateAsync(existingToken, ct);
        }
        else
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = refreshTokenValue,
                ExpiryTime = expiryTime
            };
            await refreshTokenRepo.AddAsync(refreshToken, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return new AuthTokens
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue
        };
    }

    public async Task RevokeTokenAsync(string refreshTokenValue, CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var refreshTokenRepo = unitOfWork.Repository<IRefreshTokenRepository>();

        var refreshToken = await refreshTokenRepo.GetSingleAsync(x => x.Token == refreshTokenValue, ct);
        if (refreshToken != null)
        {
            await refreshTokenRepo.DeleteAsync(refreshToken, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task<AuthTokens> RefreshTokensAsync(string refreshTokenValue, CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var refreshTokenRepo = unitOfWork.Repository<IRefreshTokenRepository>();
        var userRepo = unitOfWork.Repository<IUserRepository>();

        var refreshToken = await refreshTokenRepo.GetSingleAsync(
            x => x.Token == refreshTokenValue && x.ExpiryTime > DateTime.UtcNow,
            ct,
            x => x.User!);

        if (refreshToken == null || refreshToken.User == null)
        {
            throw new UnauthorizedException(new ApiMessage(this, "Invalid or expired refresh token."));
        }

        return await GenerateTokensAsync(refreshToken.UserId, refreshToken.User.UserName, ct);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateAccessToken(Guid userId, string userName)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshTokenValue()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
