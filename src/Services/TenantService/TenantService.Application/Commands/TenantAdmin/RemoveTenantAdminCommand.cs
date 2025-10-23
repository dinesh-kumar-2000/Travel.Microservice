using MediatR;

namespace TenantService.Application.Commands.TenantAdmin;

public class RemoveTenantAdminCommand : IRequest<Unit>
{
    public Guid AdminId { get; set; }
    public string RemovedBy { get; set; } = string.Empty;
}
