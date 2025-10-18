using SharedKernel.Models;

namespace IdentityService.Domain.Entities;

public class Role : BaseEntity<string>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private Role() { } // For Dapper

    public static Role Create(string id, string name, string description)
    {
        return new Role
        {
            Id = id,
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Common roles
    public static class Names
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string TenantAdmin = "TenantAdmin";
        public const string Agent = "Agent";
        public const string Customer = "Customer";
    }
}

