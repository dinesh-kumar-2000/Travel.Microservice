using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ReportingService.Application.DTOs.Responses.Dashboard;
using ReportingService.Application.DTOs.Requests.Dashboard;
using ReportingService.Application.Commands.Dashboard;
using ReportingService.Application.Queries.Dashboard;
using MediatR;
using SharedKernel.Models;

namespace ReportingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<DashboardResponse>>> GetAllDashboards(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllDashboardsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DashboardResponse>> GetDashboard(Guid id)
    {
        var query = new GetDashboardQuery { DashboardId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<DashboardResponse>> CreateDashboard(
        [FromBody] CreateDashboardRequest request)
    {
        var command = new CreateDashboardCommand
        {
            Name = request.Name,
            Description = request.Description,
            DashboardType = request.DashboardType,
            Layout = request.Layout,
            Widgets = request.Widgets,
            IsPublic = request.IsPublic
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetDashboard), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<DashboardResponse>> UpdateDashboard(
        Guid id, 
        [FromBody] UpdateDashboardRequest request)
    {
        var command = new UpdateDashboardCommand
        {
            DashboardId = id,
            Name = request.Name,
            Description = request.Description,
            DashboardType = request.DashboardType,
            Layout = request.Layout,
            Widgets = request.Widgets,
            IsPublic = request.IsPublic
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<IActionResult> DeleteDashboard(Guid id)
    {
        var command = new DeleteDashboardCommand { DashboardId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
