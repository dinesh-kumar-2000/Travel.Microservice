using MediatR;
using TenantService.Application.DTOs;
using TenantService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace TenantService.Application.Queries;

public class GetTenantStatsQueryHandler : IRequestHandler<GetTenantStatsQuery, TenantStatsResponse>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<GetTenantStatsQueryHandler> _logger;

    public GetTenantStatsQueryHandler(
        ITenantRepository repository,
        ILogger<GetTenantStatsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TenantStatsResponse> Handle(GetTenantStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tenant statistics");

        var stats = await _repository.GetTenantStatisticsAsync(cancellationToken);

        var tenantsByTier = new Dictionary<string, int>
        {
            { "Basic", stats.GetValueOrDefault("BasicTier", 0) },
            { "Standard", stats.GetValueOrDefault("StandardTier", 0) },
            { "Premium", stats.GetValueOrDefault("PremiumTier", 0) },
            { "Enterprise", stats.GetValueOrDefault("EnterpriseTier", 0) }
        };

        _logger.LogInformation("Retrieved tenant statistics: {TotalTenants} total tenants", 
            stats.GetValueOrDefault("TotalTenants", 0));

        return new TenantStatsResponse(
            stats.GetValueOrDefault("TotalTenants", 0),
            stats.GetValueOrDefault("ActiveTenants", 0),
            stats.GetValueOrDefault("SuspendedTenants", 0),
            stats.GetValueOrDefault("InactiveTenants", 0),
            tenantsByTier
        );
    }
}

