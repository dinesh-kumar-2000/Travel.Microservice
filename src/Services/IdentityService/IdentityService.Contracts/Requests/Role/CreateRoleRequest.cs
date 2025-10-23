using System.ComponentModel.DataAnnotations;

namespace IdentityService.Contracts.Requests.Role;

public class CreateRoleRequest
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<string> Permissions { get; set; } = new();
}
