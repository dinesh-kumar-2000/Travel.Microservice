using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ReportingService.Contracts.Responses.Report;
using ReportingService.Contracts.Requests.Export;
using ReportingService.Application.Commands.Report;
using ReportingService.Application.Queries.Report;
using MediatR;
using SharedKernel.Models;

namespace ReportingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("export")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<ExportResponse>> ExportReport(
        [FromBody] ExportReportRequest request)
    {
        var command = new ExportReportCommand
        {
            ReportId = request.ReportId,
            ExportFormat = request.ExportFormat,
            ExportOptions = request.ExportOptions
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("formats")]
    public async Task<ActionResult<List<string>>> GetExportFormats()
    {
        return Ok(new List<string> { "PDF", "Excel", "CSV", "JSON" });
    }
}
