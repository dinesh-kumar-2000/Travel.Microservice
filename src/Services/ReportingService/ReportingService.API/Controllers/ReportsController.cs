using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ReportingService.Infrastructure.Repositories;
using Identity.Shared;
using Tenancy;
using SharedKernel.Auditing;

namespace ReportingService.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize(Roles = "TenantAdmin,SuperAdmin")]
public class ReportsController : ControllerBase
{
    private readonly IReportingRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportingRepository repository,
        ITenantContext tenantContext,
        IAuditService auditService,
        ILogger<ReportsController> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get booking statistics for a date range
    /// </summary>
    [HttpGet("bookings/stats")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(BookingStats), StatusCodes.Status200OK)]
    public async Task<ActionResult<BookingStats>> GetBookingStats(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        _logger.LogInformation("Getting booking stats for tenant {TenantId} from {StartDate} to {EndDate}", 
            _tenantContext.TenantId, startDate, endDate);

        var stats = await _repository.GetBookingStatsAsync(
            _tenantContext.TenantId!,
            startDate,
            endDate);
        
        // Audit log
        await _auditService.LogAsync(new AuditEntry
        {
            TenantId = _tenantContext.TenantId!,
            UserId = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown",
            Action = "View",
            EntityType = "Report",
            EntityId = $"booking-stats-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });

        return Ok(stats);
    }

    /// <summary>
    /// Get monthly revenue breakdown for a year
    /// </summary>
    [HttpGet("revenue/monthly")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(IEnumerable<MonthlyRevenue>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MonthlyRevenue>>> GetMonthlyRevenue([FromQuery] int year)
    {
        _logger.LogInformation("Getting monthly revenue for tenant {TenantId}, year {Year}", 
            _tenantContext.TenantId, year);

        var revenue = await _repository.GetRevenueByMonthAsync(
            _tenantContext.TenantId!,
            year);

        return Ok(revenue);
    }

    /// <summary>
    /// Get top destinations by booking count
    /// </summary>
    [HttpGet("destinations/top")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(IEnumerable<TopDestination>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TopDestination>>> GetTopDestinations([FromQuery] int limit = 10)
    {
        _logger.LogInformation("Getting top {Limit} destinations for tenant {TenantId}", 
            limit, _tenantContext.TenantId);

        var destinations = await _repository.GetTopDestinationsAsync(
            _tenantContext.TenantId!,
            limit);

        return Ok(destinations);
    }
}

