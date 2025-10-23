using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SharedKernel.Models;
using IdentityService.Application.DTOs.Requests.Role;
using IdentityService.Application.DTOs.Responses.Role;
using IdentityService.Application.Commands.Role;
using IdentityService.Application.Queries.Role;
using MediatR;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RoleController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAllRoles()
    {
        var query = new GetAllRolesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleResponse>> GetRole(Guid id)
    {
        var query = new GetRoleQuery { RoleId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> CreateRole([FromBody] CreateRoleRequest request)
    {
        var command = new CreateRoleCommand
        {
            Name = request.Name,
            Description = request.Description,
            Permissions = request.Permissions
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetRole), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoleResponse>> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var command = new UpdateRoleCommand
        {
            RoleId = id,
            Name = request.Name,
            Description = request.Description,
            Permissions = request.Permissions
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var command = new DeleteRoleCommand { RoleId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
