namespace CezStudentAssistant.Infrastructure.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
    public int RefreshTokenGracePeriodSeconds { get; set; }

    public JwtSettings Validate()
    {
        if (string.IsNullOrWhiteSpace(Secret))
            throw new InvalidOperationException("Configuration 'JwtSettings:Secret' is missing or empty.");
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("Configuration 'JwtSettings:Issuer' is missing or empty.");
        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("Configuration 'JwtSettings:Audience' is missing or empty.");
        if (AccessTokenExpiryMinutes <= 0)
            throw new InvalidOperationException("Configuration 'JwtSettings:AccessTokenExpiryMinutes' is missing or invalid.");
        if (RefreshTokenExpiryDays <= 0)
            throw new InvalidOperationException("Configuration 'JwtSettings:RefreshTokenExpiryDays' is missing or invalid.");
        if (RefreshTokenGracePeriodSeconds <= 0)
            throw new InvalidOperationException("Configuration 'JwtSettings:RefreshTokenGracePeriodSeconds' is missing or invalid.");

        return this;
    }
}
