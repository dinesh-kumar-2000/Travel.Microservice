namespace IdentityService.Domain.Entities;

public class TwoFactorAuth
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Secret { get; set; } = string.Empty;
    public List<string> BackupCodes { get; set; } = new();
    public bool Enabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }

    // Navigation property
    public User? User { get; set; }
}

public class TwoFactorAuthLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

