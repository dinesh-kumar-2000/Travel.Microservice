using MediatR;
using TenantService.Contracts.Responses.TenantAdmin;

namespace TenantService.Application.Commands.TenantAdmin;

public class AssignTenantAdminCommand : IRequest<TenantAdminResponse>
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public string AssignedBy { get; set; } = string.Empty;
}
