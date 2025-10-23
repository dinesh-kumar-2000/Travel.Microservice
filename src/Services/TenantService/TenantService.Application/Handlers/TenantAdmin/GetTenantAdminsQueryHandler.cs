using MediatR;
using Microsoft.Extensions.Logging;
using TenantService.Application.Queries.TenantAdmin;
using TenantService.Contracts.Responses.TenantAdmin;
using TenantService.Domain.Interfaces;

namespace TenantService.Application.Handlers.TenantAdmin;

public class GetTenantAdminsQueryHandler : IRequestHandler<GetTenantAdminsQuery, IEnumerable<TenantAdminResponse>>
{
    private readonly ITenantAdminRepository _tenantAdminRepository;
    private readonly ILogger<GetTenantAdminsQueryHandler> _logger;

    public GetTenantAdminsQueryHandler(
        ITenantAdminRepository tenantAdminRepository,
        ILogger<GetTenantAdminsQueryHandler> logger)
    {
        _tenantAdminRepository = tenantAdminRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<TenantAdminResponse>> Handle(GetTenantAdminsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tenant admins for tenant {TenantId}", request.TenantId);

        var admins = await _tenantAdminRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        
        var result = admins.Select(admin => new TenantAdminResponse
        {
            Id = admin.Id,
            TenantId = admin.TenantId,
            UserId = admin.UserId,
            Role = admin.Role,
            CreatedAt = admin.CreatedAt,
            UpdatedAt = admin.UpdatedAt
        });
        
        _logger.LogInformation("Retrieved {Count} tenant admins for tenant {TenantId}", result.Count(), request.TenantId);
        
        return result;
    }
}
