using MediatR;
using TenantService.Application.DTOs;
using TenantService.Application.Interfaces;
using SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;

namespace TenantService.Application.Commands;

public record SuspendTenantCommand(
    string TenantId
) : IRequest<ActivateTenantResponse>;

public class SuspendTenantCommandHandler : IRequestHandler<SuspendTenantCommand, ActivateTenantResponse>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<SuspendTenantCommandHandler> _logger;

    public SuspendTenantCommandHandler(
        ITenantRepository repository,
        ILogger<SuspendTenantCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ActivateTenantResponse> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Suspending tenant {TenantId}", request.TenantId);

        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken);
        
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
            throw new NotFoundException("Tenant not found");
        }

        tenant.Suspend();
        await _repository.UpdateAsync(tenant, cancellationToken);

        _logger.LogInformation("Tenant {TenantId} suspended successfully", request.TenantId);

        return new ActivateTenantResponse(
            true,
            "Tenant suspended successfully",
            tenant.Id,
            tenant.Status.ToString()
        );
    }
}

