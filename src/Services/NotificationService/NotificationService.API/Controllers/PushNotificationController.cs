using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotificationService.Contracts.Responses.PushNotification;
using NotificationService.Contracts.Requests.PushNotification;
using NotificationService.Application.Commands.PushNotification;
using NotificationService.Application.Queries.PushNotification;
using MediatR;
using SharedKernel.Models;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PushNotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public PushNotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<PushNotificationResponse>> SendPushNotification(
        [FromBody] SendPushNotificationRequest request)
    {
        // TODO: Implement proper SendPushNotificationCommand mapping
        return Ok(new PushNotificationResponse { Id = Guid.NewGuid(), Status = 1, SentAt = DateTime.UtcNow });
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<object>> SendBulkPushNotification(
        [FromBody] SendBulkPushNotificationRequest request)
    {
        // TODO: Implement proper SendBulkPushNotificationCommand mapping
        return Ok(new { SentCount = request.DeviceTokens.Length, Status = "Sent" });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PushNotificationResponse>> GetPushNotification(Guid id)
    {
        // TODO: Implement proper GetPushNotificationById query
        return NotFound("GetPushNotificationById query not implemented yet");
    }

    [HttpGet("history/{userId}")]
    public async Task<ActionResult<PaginatedResult<PushNotificationResponse>>> GetPushNotificationHistory(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // TODO: Implement proper GetPushNotificationHistory query
        return NotFound("GetPushNotificationHistory query not implemented yet");
    }
}
