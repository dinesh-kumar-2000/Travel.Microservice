using IdentityService.Application.DTOs;
using MediatR;

namespace IdentityService.Application.Commands;

public record GenerateTwoFactorSecretCommand(Guid UserId) 
    : IRequest<TwoFactorSecretDto>;

public record VerifyAndEnableTwoFactorCommand(
    Guid UserId,
    string Secret,
    string Code
) : IRequest<TwoFactorAuthResponseDto>;

public record AuthenticateWithTwoFactorCommand(
    Guid UserId,
    string Code,
    bool IsBackupCode
) : IRequest<TwoFactorAuthResponseDto>;

public record DisableTwoFactorCommand(
    Guid UserId,
    string Password
) : IRequest<TwoFactorAuthResponseDto>;

public record RegenerateBackupCodesCommand(Guid UserId) 
    : IRequest<BackupCodesDto>;

