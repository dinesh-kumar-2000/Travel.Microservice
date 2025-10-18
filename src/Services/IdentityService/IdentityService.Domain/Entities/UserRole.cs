using SharedKernel.Models;

namespace IdentityService.Domain.Entities;

public class UserRole : BaseEntity<string>
{
    public string UserId { get; private set; } = string.Empty;
    public string RoleId { get; private set; } = string.Empty;

    private UserRole() { } // For Dapper

    public static UserRole Create(string id, string userId, string roleId)
    {
        return new UserRole
        {
            Id = id,
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        };
    }
}

