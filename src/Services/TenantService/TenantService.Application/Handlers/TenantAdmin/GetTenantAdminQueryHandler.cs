using MediatR;
using Microsoft.Extensions.Logging;
using TenantService.Application.Queries.TenantAdmin;
using TenantService.Application.DTOs.Responses.TenantAdmin;
using TenantService.Application.Interfaces;

namespace TenantService.Application.Handlers.TenantAdmin;

public class GetTenantAdminQueryHandler : IRequestHandler<GetTenantAdminQuery, TenantAdminResponse?>
{
    private readonly ITenantAdminRepository _tenantAdminRepository;
    private readonly ILogger<GetTenantAdminQueryHandler> _logger;

    public GetTenantAdminQueryHandler(
        ITenantAdminRepository tenantAdminRepository,
        ILogger<GetTenantAdminQueryHandler> logger)
    {
        _tenantAdminRepository = tenantAdminRepository;
        _logger = logger;
    }

    public async Task<TenantAdminResponse?> Handle(GetTenantAdminQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tenant admin {AdminId}", request.AdminId);

        var admin = await _tenantAdminRepository.GetByIdAsync(request.AdminId.ToString(), cancellationToken);
        
        if (admin == null)
        {
            _logger.LogWarning("Tenant admin {AdminId} not found", request.AdminId);
            return null;
        }

        _logger.LogInformation("Retrieved tenant admin {AdminId}", request.AdminId);

        return new TenantAdminResponse
        {
            Id = admin.Id,
            TenantId = admin.TenantId,
            UserId = admin.UserId,
            Role = admin.Role,
            CreatedAt = admin.CreatedAt,
            UpdatedAt = admin.UpdatedAt
        };
    }
}
