using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentService.Contracts.Responses.Refund;
using PaymentService.Contracts.Requests.Refund;
using PaymentService.Application.Commands.Refund;
using PaymentService.Application.Queries.Refund;
using MediatR;
using SharedKernel.Models;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RefundController : ControllerBase
{
    private readonly IMediator _mediator;

    public RefundController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<RefundResponse>>> GetAllRefunds(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllRefundsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RefundResponse>> GetRefund(Guid id)
    {
        var query = new GetRefundQuery { RefundId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<RefundResponse>> ProcessRefund(
        [FromBody] ProcessRefundRequest request)
    {
        var command = new ProcessRefundCommand
        {
            PaymentId = Guid.Parse(request.PaymentId),
            Amount = request.Amount,
            Currency = request.Currency,
            Reason = request.Reason,
            Reference = request.Reference
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetRefund), new { id = result.Id }, result);
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<RefundResponse>> ApproveRefund(Guid id)
    {
        var command = new ApproveRefundCommand { RefundId = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
