using System.Text.Json.Serialization;

namespace IdentityService.Contracts.DTOs;

public record RevokeTokenRequest(
    [property: JsonPropertyName("userId")] string UserId
);

public record RevokeTokenResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message
);

