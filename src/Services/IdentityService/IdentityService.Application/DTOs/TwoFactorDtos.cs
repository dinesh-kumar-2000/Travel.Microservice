namespace IdentityService.Application.DTOs;

public record TwoFactorSecretDto(
    string Secret,
    string QrCodeUrl,
    List<string> BackupCodes
);

public record TwoFactorStatusDto(
    bool Enabled,
    int BackupCodesRemaining
);

public record TwoFactorAuthResponseDto(
    bool Success,
    string? Token,
    string? Message
);

public record BackupCodesDto(
    List<string> BackupCodes
);

public record VerifyTwoFactorRequest(
    string Secret,
    string Code
);

public record TwoFactorAuthRequest(
    Guid UserId,
    string Code,
    bool IsBackupCode = false
);

public record DisableTwoFactorRequest(
    string Password
);

