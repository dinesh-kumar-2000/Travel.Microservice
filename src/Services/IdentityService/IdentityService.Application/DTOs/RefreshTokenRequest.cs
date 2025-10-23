using System.Text.Json.Serialization;

namespace IdentityService.Application.DTOs;

public record RefreshTokenRequest(
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("userId")] string UserId
);

public record RefreshTokenResponse(
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt
);

