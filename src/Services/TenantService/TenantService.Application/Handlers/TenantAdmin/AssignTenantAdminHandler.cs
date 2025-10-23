using MediatR;
using Microsoft.Extensions.Logging;
using TenantService.Application.Commands.TenantAdmin;
using TenantService.Application.DTOs.Responses.TenantAdmin;
using TenantService.Application.Interfaces;
using SharedKernel.Exceptions;

namespace TenantService.Application.Handlers.TenantAdmin;

public class AssignTenantAdminHandler : IRequestHandler<AssignTenantAdminCommand, TenantAdminResponse>
{
    private readonly ITenantAdminRepository _tenantAdminRepository;
    private readonly ILogger<AssignTenantAdminHandler> _logger;

    public AssignTenantAdminHandler(
        ITenantAdminRepository tenantAdminRepository,
        ILogger<AssignTenantAdminHandler> logger)
    {
        _tenantAdminRepository = tenantAdminRepository;
        _logger = logger;
    }

    public async Task<TenantAdminResponse> Handle(AssignTenantAdminCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Assigning admin role for user {UserId} to tenant {TenantId}", 
            request.UserId, request.TenantId);

        var tenantAdmin = Domain.Entities.TenantAdmin.Create(
            request.TenantId, 
            request.UserId, 
            request.Role, 
            request.AssignedBy);

        await _tenantAdminRepository.AddAsync(tenantAdmin, cancellationToken);
        
        _logger.LogInformation("Admin role assigned successfully for user {UserId} to tenant {TenantId}", 
            request.UserId, request.TenantId);

        return new TenantAdminResponse
        {
            Id = tenantAdmin.Id,
            TenantId = tenantAdmin.TenantId,
            UserId = tenantAdmin.UserId,
            Role = tenantAdmin.Role,
            CreatedAt = tenantAdmin.CreatedAt,
            UpdatedAt = tenantAdmin.UpdatedAt
        };
    }
}
