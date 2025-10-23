using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentService.Application.DTOs.Responses.Transaction;
using PaymentService.Application.DTOs.Requests.Transaction;
using PaymentService.Application.Commands.Transaction;
using PaymentService.Application.Queries.Transaction;
using MediatR;
using SharedKernel.Models;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<TransactionResponse>>> GetAllTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllTransactionsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionResponse>> GetTransaction(Guid id)
    {
        var query = new GetTransactionQuery { TransactionId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpGet("history/{userId}")]
    public async Task<ActionResult<PaginatedResult<TransactionResponse>>> GetTransactionHistory(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetTransactionHistoryQuery
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

    [HttpPost]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<TransactionResponse>> CreateTransaction(
        [FromBody] CreateTransactionRequest request)
    {
        var command = new CreateTransactionCommand
        {
            PaymentId = request.PaymentId,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            Currency = request.Currency,
            Description = request.Description,
            Reference = request.Reference,
            Metadata = request.Metadata
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTransaction), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,TenantAdmin")]
    public async Task<ActionResult<TransactionResponse>> UpdateTransaction(
        Guid id, 
        [FromBody] UpdateTransactionRequest request)
    {
        var command = new UpdateTransactionCommand
        {
            TransactionId = id,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            Currency = request.Currency,
            Description = request.Description,
            Reference = request.Reference,
            Metadata = request.Metadata
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
