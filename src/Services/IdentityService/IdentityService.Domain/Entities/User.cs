using SharedKernel.Models;

namespace IdentityService.Domain.Entities;

public class User : TenantEntity<string>
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public bool EmailConfirmed { get; private set; }
    public bool PhoneNumberConfirmed { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }
    public DateTime? LastPasswordChange { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public int FailedLoginAttempts { get; private set; }

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private User() { } // For Dapper

    public static User Create(string id, string tenantId, string email, string passwordHash, 
        string firstName, string lastName, string phoneNumber)
    {
        var user = new User
        {
            Id = id,
            TenantId = tenantId,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        return user;
    }

    public void UpdateProfile(string firstName, string lastName, string phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRefreshToken(string refreshToken, DateTime expiresAt)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastPasswordChange()
    {
        LastPasswordChange = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LockAccount(DateTime lockedUntil)
    {
        LockedUntil = lockedUntil;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnlockAccount()
    {
        LockedUntil = null;
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementFailedLoginAttempts()
    {
        FailedLoginAttempts++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }
}

