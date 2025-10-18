namespace IdentityService.Contracts.DTOs;

public record RegisterUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string TenantId
);

public record RegisterUserResponse(
    string UserId,
    string Email,
    string Message
);

