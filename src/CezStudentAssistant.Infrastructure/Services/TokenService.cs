using CezStudentAssistant.Application.Interfaces.Persistence;
using CezStudentAssistant.Application.Interfaces.Services;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Common;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CezStudentAssistant.Infrastructure.Services;

public class TokenService : ITokenService, IScopedService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUnitOfWork _unitOfWork;

    public TokenService(IOptions<JwtSettings> jwtOptions, IUnitOfWork unitOfWork)
    {
        _jwtSettings = jwtOptions.Value;
        _unitOfWork = unitOfWork;
    }

    public string GenerateAccessToken(Guid userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
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

    public async Task<string> HandleRefreshToken(Guid userId, CancellationToken ct)
    {
        string refreshToken = GenerateRefreshToken();

        var tokenRepo = _unitOfWork.Repository<IRefreshTokenRepository>();
        var currentToken = await tokenRepo.GetSingleAsync(x => x.UserId == userId, ct);
        if (currentToken is null)
        {
            string generatedToken = refreshToken;
            var refreshTokenEntity = new RefreshToken
            {
                Token = generatedToken,
                ExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                UserId = userId
            };
            await tokenRepo.AddAsync(refreshTokenEntity, ct);
        }
        else
        {
            currentToken.Token = refreshToken;
            currentToken.ExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
            await tokenRepo.UpdateAsync(currentToken, ct);
        }

        return refreshToken;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
