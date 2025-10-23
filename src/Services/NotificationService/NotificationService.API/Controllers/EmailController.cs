using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotificationService.Application.DTOs.Responses.Email;
using NotificationService.Application.DTOs.Requests.Email;
using NotificationService.Application.Commands.Email;
using NotificationService.Application.Queries.Email;
using MediatR;
using SharedKernel.Models;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmailController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<EmailResponse>> SendEmail(
        [FromBody] SendEmailRequest request)
    {
        // TODO: Implement proper SendEmailCommand mapping
        return Ok(new EmailResponse { Id = Guid.NewGuid(), Status = 1, SentAt = DateTime.UtcNow });
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<object>> SendBulkEmail(
        [FromBody] SendBulkEmailRequest request)
    {
        // TODO: Implement proper SendBulkEmailCommand mapping
        return Ok(new { SentCount = request.To.Length, Status = "Sent" });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmailResponse>> GetEmail(Guid id)
    {
        // TODO: Implement proper GetEmailById query
        return NotFound("GetEmailById query not implemented yet");
    }

    [HttpGet("history/{userId}")]
    public async Task<ActionResult<PaginatedResult<EmailResponse>>> GetEmailHistory(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // TODO: Implement proper GetEmailHistory query
        return NotFound("GetEmailHistory query not implemented yet");
    }
}
