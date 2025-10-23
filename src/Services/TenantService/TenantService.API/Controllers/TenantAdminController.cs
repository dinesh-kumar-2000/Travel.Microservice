using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TenantService.Contracts.Responses.TenantAdmin;
using TenantService.Contracts.Requests.TenantAdmin;
using TenantService.Application.Commands.TenantAdmin;
using TenantService.Application.Queries.TenantAdmin;
using MediatR;

namespace TenantService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TenantAdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantAdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult<IEnumerable<TenantAdminResponse>>> GetTenantAdmins(Guid tenantId)
    {
        var query = new GetTenantAdminsQuery { TenantId = tenantId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{adminId}")]
    public async Task<ActionResult<TenantAdminResponse>> GetTenantAdmin(Guid adminId)
    {
        var query = new GetTenantAdminQuery { AdminId = adminId };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost("assign")]
    public async Task<ActionResult<TenantAdminResponse>> AssignTenantAdmin(
        [FromBody] AssignTenantAdminRequest request)
    {
        var command = new AssignTenantAdminCommand
        {
            TenantId = request.TenantId,
            UserId = request.UserId,
            Permissions = request.Permissions,
            AssignedBy = User.Identity?.Name ?? "System"
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTenantAdmin), new { adminId = result.Id }, result);
    }

    [HttpDelete("{adminId}")]
    public async Task<IActionResult> RemoveTenantAdmin(Guid adminId)
    {
        var command = new RemoveTenantAdminCommand 
        { 
            AdminId = adminId,
            RemovedBy = User.Identity?.Name ?? "System"
        };
        
        await _mediator.Send(command);
        return NoContent();
    }
}
