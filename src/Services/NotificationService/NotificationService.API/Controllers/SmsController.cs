using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotificationService.Contracts.Responses.Sms;
using NotificationService.Contracts.Requests.Sms;
using NotificationService.Application.Commands.Sms;
using NotificationService.Application.Queries.Sms;
using MediatR;
using SharedKernel.Models;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SmsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SmsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<SmsResponse>> SendSms(
        [FromBody] SendSmsRequest request)
    {
        // TODO: Implement proper SendSmsCommand mapping
        return Ok(new SmsResponse { Id = Guid.NewGuid(), Status = 1, SentAt = DateTime.UtcNow });
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<object>> SendBulkSms(
        [FromBody] SendBulkSmsRequest request)
    {
        // TODO: Implement proper SendBulkSmsCommand mapping
        return Ok(new { SentCount = request.To.Length, Status = "Sent" });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SmsResponse>> GetSms(Guid id)
    {
        // TODO: Implement proper GetSmsById query
        return NotFound("GetSmsById query not implemented yet");
    }

    [HttpGet("history/{userId}")]
    public async Task<ActionResult<PaginatedResult<SmsResponse>>> GetSmsHistory(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // TODO: Implement proper GetSmsHistory query
        return NotFound("GetSmsHistory query not implemented yet");
    }
}
