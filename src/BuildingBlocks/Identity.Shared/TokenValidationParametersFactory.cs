using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Identity.Shared;

/// <summary>
/// Factory for creating consistent TokenValidationParameters across the application
/// </summary>
public static class TokenValidationParametersFactory
{
    /// <summary>
    /// Creates TokenValidationParameters with standard configuration
    /// </summary>
    public static TokenValidationParameters Create(JwtSettings settings)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    /// <summary>
    /// Creates TokenValidationParameters with custom clock skew
    /// </summary>
    public static TokenValidationParameters Create(JwtSettings settings, TimeSpan clockSkew)
    {
        var parameters = Create(settings);
        parameters.ClockSkew = clockSkew;
        return parameters;
    }
}

