using TenantService.Application.Commands;
using TenantService.Application.Queries;
using TenantService.Contracts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TenantService.API.Controllers;

[ApiController]
[Route("api/tenantadmin/templates")]
[Authorize(Roles = "TenantAdmin")]
public class TemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(IMediator mediator, ILogger<TemplatesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all templates (email and SMS)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<TemplatesResponseDto>> GetTemplates([FromQuery] string? type = null)
    {
        var tenantId = GetTenantId();
        var query = new GetTemplatesQuery(tenantId, type);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get template by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TemplateDto>> GetTemplate(Guid id)
    {
        var tenantId = GetTenantId();
        var query = new GetTemplateByIdQuery(id, tenantId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create email template
    /// </summary>
    [HttpPost("email")]
    public async Task<ActionResult<EmailTemplateDto>> CreateEmailTemplate([FromBody] CreateEmailTemplateRequest request)
    {
        var tenantId = GetTenantId();

        var command = new CreateEmailTemplateCommand(
            tenantId,
            request.Name,
            request.TemplateType,
            request.Category,
            request.Subject,
            request.Content,
            request.Variables,
            request.IsActive
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTemplate), new { id = result.Id }, result);
    }

    /// <summary>
    /// Create SMS template
    /// </summary>
    [HttpPost("sms")]
    public async Task<ActionResult<SMSTemplateDto>> CreateSMSTemplate([FromBody] CreateSMSTemplateRequest request)
    {
        var tenantId = GetTenantId();

        var command = new CreateSMSTemplateCommand(
            tenantId,
            request.Name,
            request.TemplateType,
            request.Category,
            request.Content,
            request.Variables,
            request.IsActive
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTemplate), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update template
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TemplateDto>> UpdateTemplate(Guid id, [FromBody] UpdateTemplateRequest request)
    {
        var tenantId = GetTenantId();

        var command = new UpdateTemplateCommand(
            id,
            tenantId,
            request.Name,
            request.Subject,
            request.Content,
            request.Variables,
            request.IsActive
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete template
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTemplate(Guid id)
    {
        var tenantId = GetTenantId();
        var command = new DeleteTemplateCommand(id, tenantId);
        await _mediator.Send(command);

        return NoContent();
    }

    private Guid GetTenantId()
    {
        var tenantId = User.FindFirst("tenantId")?.Value;
        return string.IsNullOrEmpty(tenantId) ? Guid.Empty : Guid.Parse(tenantId);
    }
}

