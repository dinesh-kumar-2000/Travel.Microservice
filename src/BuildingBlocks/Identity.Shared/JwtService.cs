using Microsoft.IdentityModel.Tokens;
using SharedKernel.Constants;
using SharedKernel.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Shared;

public interface IJwtService
{
    string GenerateToken(string userId, string email, string tenantId, IEnumerable<string> roles);
    ClaimsPrincipal? ValidateToken(string token);
}

public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIdGenerator _idGenerator;

    public JwtService(JwtSettings settings, IDateTimeProvider dateTimeProvider, IIdGenerator idGenerator)
    {
        _settings = settings;
        _dateTimeProvider = dateTimeProvider;
        _idGenerator = idGenerator;
    }

    public string GenerateToken(string userId, string email, string tenantId, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ApplicationConstants.Claims.TenantId, tenantId),
            new(JwtRegisteredClaimNames.Jti, _idGenerator.Generate())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: _dateTimeProvider.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = TokenValidationParametersFactory.Create(_settings);
            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }
}

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}

