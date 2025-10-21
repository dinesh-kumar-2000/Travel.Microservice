using System.Text.Json.Serialization;

namespace IdentityService.Contracts.DTOs;

public record GetProfileRequest(
    [property: JsonPropertyName("userId")] string UserId
);

public record GetProfileResponse(
    [property: JsonPropertyName("user")] UserDto User
);

public record UpdateProfileRequest(
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("phoneNumber")] string PhoneNumber
);

public record UpdateProfileResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("user")] UserDto User
);

