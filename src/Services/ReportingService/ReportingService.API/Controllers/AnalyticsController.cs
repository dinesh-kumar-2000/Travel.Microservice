using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ReportingService.Application.DTOs.Responses.Analytics;
using ReportingService.Application.DTOs.Requests.Analytics;
using ReportingService.Application.Commands.Analytics;
using ReportingService.Application.Queries.Analytics;
using MediatR;
using SharedKernel.Models;

namespace ReportingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("generate")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<AnalyticsResponse>> GenerateAnalytics(
        [FromBody] GenerateAnalyticsRequest request)
    {
        var command = new GenerateAnalyticsCommand
        {
            AnalyticsType = request.AnalyticsType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Filters = request.Filters,
            GroupBy = request.GroupBy
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<AnalyticsResponse>> UpdateAnalytics(
        Guid id, 
        [FromBody] UpdateAnalyticsRequest request)
    {
        var command = new UpdateAnalyticsCommand
        {
            AnalyticsId = id,
            AnalyticsType = request.AnalyticsType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Filters = request.Filters,
            GroupBy = request.GroupBy
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnalyticsResponse>> GetAnalytics(Guid id)
    {
        var query = new GetAnalyticsQuery { AnalyticsId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpGet("data/{id}")]
    public async Task<ActionResult<AnalyticsDataResponse>> GetAnalyticsData(Guid id)
    {
        var query = new GetAnalyticsDataQuery { AnalyticsId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }
}
