namespace IdentityService.Contracts.DTOs;

public record LoginRequest(
    string Email,
    string Password,
    string TenantId
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    IEnumerable<string> Roles
);

