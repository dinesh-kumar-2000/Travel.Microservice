using IdentityService.Application.Commands;
using IdentityService.Contracts.DTOs;
using IdentityService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using OtpNet;
using QRCoder;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Application.Handlers;

public class GenerateTwoFactorSecretCommandHandler 
    : IRequestHandler<GenerateTwoFactorSecretCommand, TwoFactorSecretDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GenerateTwoFactorSecretCommandHandler> _logger;

    public GenerateTwoFactorSecretCommandHandler(
        IUserRepository userRepository,
        ILogger<GenerateTwoFactorSecretCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<TwoFactorSecretDto> Handle(
        GenerateTwoFactorSecretCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new Exception("User not found");
        }

        // Generate secret key
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secretKey);

        // Generate QR code URL
        var issuer = "TravelSphere";
        var qrCodeUrl = $"otpauth://totp/{issuer}:{user.Email}?secret={base32Secret}&issuer={issuer}";

        // Generate backup codes
        var backupCodes = GenerateBackupCodes(10);

        // Store in database (encrypted)
        // TODO: Implement storage
        await _userRepository.SaveTwoFactorSecretAsync(request.UserId, base32Secret, backupCodes);

        return new TwoFactorSecretDto(base32Secret, qrCodeUrl, backupCodes);
    }

    private List<string> GenerateBackupCodes(int count)
    {
        var codes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var code = $"{GenerateRandomNumber(4)}-{GenerateRandomNumber(4)}-{GenerateRandomNumber(4)}";
            codes.Add(code);
        }
        return codes;
    }

    private string GenerateRandomNumber(int length)
    {
        var random = new Random();
        var number = "";
        for (int i = 0; i < length; i++)
        {
            number += random.Next(0, 10).ToString();
        }
        return number;
    }
}

public class VerifyAndEnableTwoFactorCommandHandler 
    : IRequestHandler<VerifyAndEnableTwoFactorCommand, TwoFactorAuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<VerifyAndEnableTwoFactorCommandHandler> _logger;

    public VerifyAndEnableTwoFactorCommandHandler(
        IUserRepository userRepository,
        ILogger<VerifyAndEnableTwoFactorCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<TwoFactorAuthResponseDto> Handle(
        VerifyAndEnableTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        // Verify the code
        var secretBytes = Base32Encoding.ToBytes(request.Secret);
        var totp = new Totp(secretBytes);
        var isValid = totp.VerifyTotp(request.Code, out _, new VerificationWindow(2, 2));

        if (!isValid)
        {
            _logger.LogWarning("Invalid 2FA code for user {UserId}", request.UserId);
            return new TwoFactorAuthResponseDto(false, null, "Invalid verification code");
        }

        // Enable 2FA for user
        await _userRepository.EnableTwoFactorAsync(request.UserId, request.Secret);

        _logger.LogInformation("2FA enabled for user {UserId}", request.UserId);
        return new TwoFactorAuthResponseDto(true, null, "2FA enabled successfully");
    }
}

public class AuthenticateWithTwoFactorCommandHandler 
    : IRequestHandler<AuthenticateWithTwoFactorCommand, TwoFactorAuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthenticateWithTwoFactorCommandHandler> _logger;
    // TODO: Add JWT token service

    public AuthenticateWithTwoFactorCommandHandler(
        IUserRepository userRepository,
        ILogger<AuthenticateWithTwoFactorCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<TwoFactorAuthResponseDto> Handle(
        AuthenticateWithTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        var twoFactorAuth = await _userRepository.GetTwoFactorAuthAsync(request.UserId);
        if (twoFactorAuth == null || !twoFactorAuth.Enabled)
        {
            return new TwoFactorAuthResponseDto(false, null, "2FA not enabled for this user");
        }

        bool isValid;

        if (request.IsBackupCode)
        {
            // Verify backup code
            isValid = await _userRepository.VerifyBackupCodeAsync(request.UserId, request.Code);
        }
        else
        {
            // Verify TOTP code
            var secretBytes = Base32Encoding.ToBytes(twoFactorAuth.Secret);
            var totp = new Totp(secretBytes);
            isValid = totp.VerifyTotp(request.Code, out _, new VerificationWindow(2, 2));
        }

        if (!isValid)
        {
            _logger.LogWarning("Invalid 2FA authentication attempt for user {UserId}", request.UserId);
            await _userRepository.LogTwoFactorActivityAsync(request.UserId, "failed", false);
            return new TwoFactorAuthResponseDto(false, null, "Invalid code");
        }

        // Generate JWT token
        // TODO: Implement JWT token generation
        var token = "jwt-token-placeholder";

        await _userRepository.LogTwoFactorActivityAsync(request.UserId, "verified", true);
        await _userRepository.UpdateTwoFactorLastUsedAsync(request.UserId);

        _logger.LogInformation("2FA authentication successful for user {UserId}", request.UserId);
        return new TwoFactorAuthResponseDto(true, token, "Authentication successful");
    }
}

public class DisableTwoFactorCommandHandler 
    : IRequestHandler<DisableTwoFactorCommand, TwoFactorAuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DisableTwoFactorCommandHandler> _logger;

    public DisableTwoFactorCommandHandler(
        IUserRepository userRepository,
        ILogger<DisableTwoFactorCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<TwoFactorAuthResponseDto> Handle(
        DisableTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        // Verify password first
        var isPasswordValid = await _userRepository.VerifyPasswordAsync(request.UserId, request.Password);
        if (!isPasswordValid)
        {
            return new TwoFactorAuthResponseDto(false, null, "Invalid password");
        }

        // Disable 2FA
        await _userRepository.DisableTwoFactorAsync(request.UserId);
        await _userRepository.LogTwoFactorActivityAsync(request.UserId, "disabled", true);

        _logger.LogInformation("2FA disabled for user {UserId}", request.UserId);
        return new TwoFactorAuthResponseDto(true, null, "2FA disabled successfully");
    }
}

public class RegenerateBackupCodesCommandHandler 
    : IRequestHandler<RegenerateBackupCodesCommand, BackupCodesDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RegenerateBackupCodesCommandHandler> _logger;

    public RegenerateBackupCodesCommandHandler(
        IUserRepository userRepository,
        ILogger<RegenerateBackupCodesCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<BackupCodesDto> Handle(
        RegenerateBackupCodesCommand request,
        CancellationToken cancellationToken)
    {
        var backupCodes = GenerateBackupCodes(10);
        await _userRepository.UpdateBackupCodesAsync(request.UserId, backupCodes);

        _logger.LogInformation("Backup codes regenerated for user {UserId}", request.UserId);
        return new BackupCodesDto(backupCodes);
    }

    private List<string> GenerateBackupCodes(int count)
    {
        var codes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var code = $"{GenerateRandomNumber(4)}-{GenerateRandomNumber(4)}-{GenerateRandomNumber(4)}";
            codes.Add(code);
        }
        return codes;
    }

    private string GenerateRandomNumber(int length)
    {
        var random = new Random();
        var number = "";
        for (int i = 0; i < length; i++)
        {
            number += random.Next(0, 10).ToString();
        }
        return number;
    }
}

