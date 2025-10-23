using NotificationService.Application.Commands;
using NotificationService.Application.Queries;
using NotificationService.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/user/support")]
[Authorize]
public class SupportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SupportController> _logger;

    public SupportController(IMediator mediator, ILogger<SupportController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all support tickets for user
    /// </summary>
    [HttpGet("tickets")]
    public async Task<ActionResult<List<SupportTicketDto>>> GetTickets([FromQuery] string? status = null)
    {
        var userId = GetUserId();

        var query = new GetUserTicketsQuery(userId, status);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get support ticket by ID
    /// </summary>
    [HttpGet("tickets/{id}")]
    public async Task<ActionResult<SupportTicketDetailDto>> GetTicket(Guid id)
    {
        var userId = GetUserId();

        var query = new GetTicketByIdQuery(id, userId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create new support ticket
    /// </summary>
    [HttpPost("tickets")]
    public async Task<ActionResult<SupportTicketDto>> CreateTicket([FromBody] CreateTicketRequest request)
    {
        var userId = GetUserId();
        var tenantId = GetTenantId();

        var command = new CreateSupportTicketCommand(
            userId,
            tenantId,
            request.Subject,
            request.Category,
            request.Priority,
            request.Description,
            request.BookingId
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTicket), new { id = result.Id }, result);
    }

    /// <summary>
    /// Add message to support ticket
    /// </summary>
    [HttpPost("tickets/{id}/messages")]
    public async Task<ActionResult<SupportMessageDto>> AddMessage(
        Guid id,
        [FromBody] AddMessageRequest request)
    {
        var userId = GetUserId();
        var userName = GetUserName();

        var command = new AddTicketMessageCommand(
            id,
            userId,
            userName,
            "user",
            request.Content,
            request.Attachments
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Close support ticket
    /// </summary>
    [HttpPost("tickets/{id}/close")]
    public async Task<ActionResult> CloseTicket(Guid id, [FromBody] CloseTicketRequest request)
    {
        var userId = GetUserId();

        var command = new CloseTicketCommand(
            id,
            userId,
            request.SatisfactionRating,
            request.SatisfactionFeedback
        );

        await _mediator.Send(command);

        return Ok(new { message = "Ticket closed successfully" });
    }

    /// <summary>
    /// Get ticket messages
    /// </summary>
    [HttpGet("tickets/{id}/messages")]
    public async Task<ActionResult<List<SupportMessageDto>>> GetMessages(Guid id)
    {
        var userId = GetUserId();

        var query = new GetTicketMessagesQuery(id, userId);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Get canned responses (for admin)
    /// </summary>
    [HttpGet("canned-responses")]
    [Authorize(Roles = "TenantAdmin,Agent")]
    public async Task<ActionResult<List<CannedResponseDto>>> GetCannedResponses([FromQuery] string? category = null)
    {
        var tenantId = GetTenantId();

        var query = new GetCannedResponsesQuery(tenantId, category);
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        return string.IsNullOrEmpty(userId) ? Guid.Empty : Guid.Parse(userId);
    }

    private Guid GetTenantId()
    {
        var tenantId = User.FindFirst("tenantId")?.Value;
        return string.IsNullOrEmpty(tenantId) ? Guid.Empty : Guid.Parse(tenantId);
    }

    private string GetUserName()
    {
        return User.FindFirst("name")?.Value ?? User.FindFirst("email")?.Value ?? "User";
    }
}

