using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotificationService.Application.DTOs.Responses.Notification;
using NotificationService.Application.DTOs.Requests.Notification;
using NotificationService.Application.Commands.Notification;
using NotificationService.Application.Queries.Notification;
using MediatR;
using SharedKernel.Models;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<NotificationResponse>>> GetAllNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllNotificationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationResponse>> GetNotification(Guid id)
    {
        var query = new GetNotificationQuery { NotificationId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<PaginatedResult<NotificationResponse>>> GetUserNotifications(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetUserNotificationsQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("user/{userId}/unread")]
    public async Task<ActionResult<PaginatedResult<NotificationResponse>>> GetUnreadNotifications(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetUnreadNotificationsQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<NotificationResponse>> SendNotification(
        [FromBody] SendNotificationRequest request)
    {
        // TODO: Implement proper SendNotificationCommand mapping
        var result = new NotificationResponse { Id = Guid.NewGuid(), SentAt = DateTime.UtcNow };
        return CreatedAtAction(nameof(GetNotification), new { id = result.Id }, result);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var command = new MarkAsReadCommand { NotificationId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var command = new DeleteNotificationCommand { NotificationId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
