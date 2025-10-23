using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TenantService.Contracts.Responses.TenantConfiguration;
using TenantService.Contracts.Requests.TenantConfiguration;
using TenantService.Application.Commands.TenantConfiguration;
using TenantService.Application.Queries.TenantConfiguration;
using MediatR;

namespace TenantService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantConfigurationController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantConfigurationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{tenantId}")]
    public async Task<ActionResult<TenantConfigurationResponse>> GetTenantConfiguration(Guid tenantId)
    {
        var query = new GetTenantConfigurationQuery { TenantId = tenantId };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPut("{tenantId}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<TenantConfigurationResponse>> UpdateTenantConfiguration(
        Guid tenantId, 
        [FromBody] UpdateTenantConfigurationRequest request)
    {
        var command = new UpdateTenantConfigurationCommand
        {
            TenantId = tenantId,
            ThemeSettings = request.ThemeSettings,
            FeatureFlags = request.FeatureFlags,
            CustomSettings = request.CustomSettings,
            BrandingSettings = request.BrandingSettings
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{tenantId}/reset")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TenantConfigurationResponse>> ResetTenantConfiguration(Guid tenantId)
    {
        var command = new ResetTenantConfigurationCommand { TenantId = tenantId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
