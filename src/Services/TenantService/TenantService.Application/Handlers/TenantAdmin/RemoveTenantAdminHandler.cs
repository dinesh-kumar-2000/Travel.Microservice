using MediatR;
using Microsoft.Extensions.Logging;
using TenantService.Application.Commands.TenantAdmin;
using TenantService.Application.Interfaces;
using SharedKernel.Exceptions;

namespace TenantService.Application.Handlers.TenantAdmin;

public class RemoveTenantAdminHandler : IRequestHandler<RemoveTenantAdminCommand, Unit>
{
    private readonly ITenantAdminRepository _tenantAdminRepository;
    private readonly ILogger<RemoveTenantAdminHandler> _logger;

    public RemoveTenantAdminHandler(
        ITenantAdminRepository tenantAdminRepository,
        ILogger<RemoveTenantAdminHandler> logger)
    {
        _tenantAdminRepository = tenantAdminRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(RemoveTenantAdminCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing tenant admin {AdminId} by {RemovedBy}", request.AdminId, request.RemovedBy);

        var tenantAdmin = await _tenantAdminRepository.GetByIdAsync(request.AdminId.ToString(), cancellationToken);
        if (tenantAdmin == null)
        {
            throw new NotFoundException($"Tenant admin with ID {request.AdminId} not found");
        }

        await _tenantAdminRepository.DeleteAsync(request.AdminId.ToString(), cancellationToken);
        
        _logger.LogInformation("Tenant admin {AdminId} removed successfully", request.AdminId);
        
        return Unit.Value;
    }
}
