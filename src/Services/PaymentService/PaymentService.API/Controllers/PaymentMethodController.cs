using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentService.Application.DTOs.Responses.PaymentMethod;
using PaymentService.Application.DTOs.Requests.PaymentMethod;
using PaymentService.Application.Commands.PaymentMethod;
using PaymentService.Application.Queries.PaymentMethod;
using MediatR;
using SharedKernel.Models;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentMethodController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentMethodController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<PaymentMethodResponse>>> GetAllPaymentMethods(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllPaymentMethodsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentMethodResponse>> GetPaymentMethod(Guid id)
    {
        var query = new GetPaymentMethodQuery { PaymentMethodId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<PaginatedResult<PaymentMethodResponse>>> GetUserPaymentMethods(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetUserPaymentMethodsQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentMethodResponse>> AddPaymentMethod(
        [FromBody] AddPaymentMethodRequest request)
    {
        var command = new AddPaymentMethodCommand
        {
            UserId = request.UserId,
            PaymentMethodType = request.PaymentMethodType,
            CardNumber = request.CardNumber,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            CardHolderName = request.CardHolderName,
            IsDefault = request.IsDefault
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPaymentMethod), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PaymentMethodResponse>> UpdatePaymentMethod(
        Guid id, 
        [FromBody] UpdatePaymentMethodRequest request)
    {
        var command = new UpdatePaymentMethodCommand
        {
            PaymentMethodId = id,
            PaymentMethodType = request.PaymentMethodType,
            CardNumber = request.CardNumber,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            CardHolderName = request.CardHolderName,
            IsDefault = request.IsDefault
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemovePaymentMethod(Guid id)
    {
        var command = new RemovePaymentMethodCommand { PaymentMethodId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
