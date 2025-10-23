using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CatalogService.Application.Interfaces;
using CatalogService.Application.DTOs;
using SharedKernel.Utilities;
using Identity.Shared;
using Tenancy;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class PackagesController : ControllerBase
{
    private readonly IPackageRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<PackagesController> _logger;

    public PackagesController(
        IPackageRepository repository,
        ICurrentUserService currentUser,
        ITenantContext tenantContext,
        ILogger<PackagesController> logger)
    {
        _repository = repository;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Get package by ID
    /// </summary>
    [HttpGet("{id}")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageDto>> GetById(string id)
    {
        _logger.LogInformation("Getting package {PackageId} for tenant {TenantId}", 
            id, _tenantContext.TenantId);

        var package = await _repository.GetByIdAsync(id);
        
        if (package == null)
            return NotFound();

        return Ok(new PackageDto(
            package.Id,
            package.Name,
            package.Description,
            package.Destination,
            package.DurationDays,
            package.Price,
            package.Currency,
            package.AvailableSlots,
            package.Status.ToString()
        ));
    }

    /// <summary>
    /// Search packages with filters
    /// </summary>
    [HttpGet("search")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> Search(
        [FromQuery] string? destination,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Searching packages for tenant {TenantId}", _tenantContext.TenantId);

        var result = await _repository.SearchAsync(
            _tenantContext.TenantId!,
            destination,
            minPrice,
            maxPrice,
            pageNumber,
            pageSize);

        return Ok(new
        {
            items = result.Items.Select(p => new PackageDto(
                p.Id,
                p.Name,
                p.Description,
                p.Destination,
                p.DurationDays,
                p.Price,
                p.Currency,
                p.AvailableSlots,
                p.Status.ToString()
            )),
            totalCount = result.TotalCount,
            pageNumber = result.PageNumber,
            pageSize = result.PageSize,
            totalPages = result.TotalPages
        });
    }

    /// <summary>
    /// Create new package (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "TenantAdmin,Agent")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PackageDto>> Create([FromBody] CreatePackageRequest request)
    {
        _logger.LogInformation("Creating package {Name} for tenant {TenantId}", 
            request.Name, _tenantContext.TenantId);

        var packageId = UlidGenerator.Generate();
        var package = CatalogService.Domain.Entities.Package.Create(
            packageId,
            _tenantContext.TenantId!,
            request.Name,
            request.Description,
            request.Destination,
            request.DurationDays,
            request.Price,
            request.MaxCapacity,
            request.StartDate,
            request.EndDate
        );

        await _repository.AddAsync(package);

        var dto = new PackageDto(
            package.Id,
            package.Name,
            package.Description,
            package.Destination,
            package.DurationDays,
            package.Price,
            package.Currency,
            package.AvailableSlots,
            package.Status.ToString()
        );

        return CreatedAtAction(nameof(GetById), new { id = packageId }, dto);
    }
}

