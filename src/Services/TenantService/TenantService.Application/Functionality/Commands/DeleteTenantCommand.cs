using MediatR;
using TenantService.Application.Interfaces;
using SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;

namespace TenantService.Application.Commands;

public record DeleteTenantCommand(
    string TenantId
) : IRequest<bool>;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, bool>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<DeleteTenantCommandHandler> _logger;

    public DeleteTenantCommandHandler(
        ITenantRepository repository,
        ILogger<DeleteTenantCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting tenant {TenantId}", request.TenantId);

        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken);
        
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
            throw new NotFoundException("Tenant not found");
        }

        await _repository.DeleteAsync(request.TenantId, cancellationToken);

        _logger.LogInformation("Tenant {TenantId} deleted successfully", request.TenantId);

        return true;
    }
}

