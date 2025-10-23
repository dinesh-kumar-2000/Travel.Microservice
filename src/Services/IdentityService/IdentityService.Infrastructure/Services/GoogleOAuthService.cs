using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;

namespace IdentityService.Infrastructure.Services;

/// <summary>
/// Google OAuth service implementation
/// </summary>
public class GoogleOAuthService : ISocialLoginService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleOAuthSettings _googleSettings;
    private readonly ILogger<GoogleOAuthService> _logger;

    public GoogleOAuthService(
        HttpClient httpClient,
        IOptions<GoogleOAuthSettings> googleSettings,
        ILogger<GoogleOAuthService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _googleSettings = googleSettings?.Value ?? throw new ArgumentNullException(nameof(googleSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SocialLoginResult> AuthenticateAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify the access token with Google
            var userInfo = await GetUserInfoFromGoogleAsync(accessToken, cancellationToken);
            
            if (userInfo == null)
            {
                return SocialLoginResult.Failed("Invalid access token");
            }

            // Extract user information
            var socialUser = new SocialUser
            {
                Provider = "Google",
                ProviderId = userInfo.Id,
                Email = userInfo.Email,
                FirstName = userInfo.GivenName,
                LastName = userInfo.FamilyName,
                ProfilePictureUrl = userInfo.Picture,
                EmailVerified = userInfo.VerifiedEmail
            };

            _logger.LogInformation("Successfully authenticated user {Email} via Google OAuth", userInfo.Email);
            
            return SocialLoginResult.Success(socialUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during Google OAuth authentication");
            return SocialLoginResult.Failed("Authentication failed");
        }
    }

    public async Task<SocialLoginResult> AuthenticateWithCodeAsync(string authorizationCode, string redirectUri, CancellationToken cancellationToken = default)
    {
        try
        {
            // Exchange authorization code for access token
            var tokenResponse = await ExchangeCodeForTokenAsync(authorizationCode, redirectUri, cancellationToken);
            
            if (tokenResponse == null)
            {
                return SocialLoginResult.Failed("Failed to exchange authorization code for token");
            }

            // Use the access token to get user info
            return await AuthenticateAsync(tokenResponse.AccessToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during Google OAuth code exchange");
            return SocialLoginResult.Failed("Authentication failed");
        }
    }

    private async Task<GoogleUserInfo?> GetUserInfoFromGoogleAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user info from Google. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting user info from Google");
            return null;
        }
    }

    private async Task<GoogleTokenResponse?> ExchangeCodeForTokenAsync(string authorizationCode, string redirectUri, CancellationToken cancellationToken)
    {
        try
        {
            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _googleSettings.ClientId),
                new KeyValuePair<string, string>("client_secret", _googleSettings.ClientSecret),
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            });

            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to exchange authorization code for token. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while exchanging authorization code for token");
            return null;
        }
    }

    public string GetAuthorizationUrl(string redirectUri, string state)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _googleSettings.ClientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["state"] = state,
            ["access_type"] = "offline",
            ["prompt"] = "consent"
        };

        var queryString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        return $"https://accounts.google.com/o/oauth2/v2/auth?{queryString}";
    }
}

/// <summary>
/// Social login service interface
/// </summary>
public interface ISocialLoginService
{
    Task<SocialLoginResult> AuthenticateAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<SocialLoginResult> AuthenticateWithCodeAsync(string authorizationCode, string redirectUri, CancellationToken cancellationToken = default);
    string GetAuthorizationUrl(string redirectUri, string state);
}

/// <summary>
/// Google OAuth settings
/// </summary>
public class GoogleOAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Google user info model
/// </summary>
public class GoogleUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public bool VerifiedEmail { get; set; }
}

/// <summary>
/// Google token response model
/// </summary>
public class GoogleTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

/// <summary>
/// Social user model
/// </summary>
public class SocialUser
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public bool EmailVerified { get; set; }
}

/// <summary>
/// Social login result
/// </summary>
public class SocialLoginResult
{
    public bool IsSuccess { get; private set; }
    public SocialUser? User { get; private set; }
    public string? ErrorMessage { get; private set; }

    private SocialLoginResult(bool isSuccess, SocialUser? user = null, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        User = user;
        ErrorMessage = errorMessage;
    }

    public static SocialLoginResult Success(SocialUser user) => new(true, user);
    public static SocialLoginResult Failed(string errorMessage) => new(false, null, errorMessage);
}
