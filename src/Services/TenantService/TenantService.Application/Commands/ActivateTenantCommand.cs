using MediatR;
using TenantService.Contracts.DTOs;
using TenantService.Domain.Repositories;
using SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;

namespace TenantService.Application.Commands;

public record ActivateTenantCommand(
    string TenantId
) : IRequest<ActivateTenantResponse>;

public class ActivateTenantCommandHandler : IRequestHandler<ActivateTenantCommand, ActivateTenantResponse>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<ActivateTenantCommandHandler> _logger;

    public ActivateTenantCommandHandler(
        ITenantRepository repository,
        ILogger<ActivateTenantCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ActivateTenantResponse> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating tenant {TenantId}", request.TenantId);

        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken);
        
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
            throw new NotFoundException("Tenant not found");
        }

        tenant.Activate();
        await _repository.UpdateAsync(tenant, cancellationToken);

        _logger.LogInformation("Tenant {TenantId} activated successfully", request.TenantId);

        return new ActivateTenantResponse(
            true,
            "Tenant activated successfully",
            tenant.Id,
            tenant.Status.ToString()
        );
    }
}

