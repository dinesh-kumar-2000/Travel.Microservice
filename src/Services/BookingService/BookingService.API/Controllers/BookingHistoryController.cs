using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookingService.Application.DTOs.Responses.BookingHistory;
using BookingService.Application.Queries.BookingHistory;
using MediatR;
using SharedKernel.Models;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingHistoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingHistoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<PaginatedResult<BookingHistoryResponseDto>>> GetUserBookingHistory(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetBookingHistoryQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("statistics/{userId}")]
    public async Task<ActionResult<BookingStatisticsResponseDto>> GetUserBookingStatistics(Guid userId)
    {
        var query = new GetBookingStatisticsQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<BookingStatisticsResponseDto>> GetOverallBookingStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetBookingStatisticsQuery 
        { 
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
