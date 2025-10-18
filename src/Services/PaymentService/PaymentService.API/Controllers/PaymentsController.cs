using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PaymentService.Domain.Repositories;
using PaymentService.Contracts.DTOs;
using Identity.Shared;
using Tenancy;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentRepository repository,
        ICurrentUserService currentUser,
        ITenantContext tenantContext,
        ILogger<PaymentsController> logger)
    {
        _repository = repository;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetById(string id)
    {
        _logger.LogInformation("Getting payment {PaymentId} for tenant {TenantId}", 
            id, _tenantContext.TenantId);

        var payment = await _repository.GetByIdAsync(id);
        
        if (payment == null)
            return NotFound();

        return Ok(new PaymentDto(
            payment.Id,
            payment.BookingId,
            payment.Amount,
            payment.Currency,
            payment.Status.ToString(),
            payment.TransactionId
        ));
    }

    /// <summary>
    /// Get payment by booking ID
    /// </summary>
    [HttpGet("booking/{bookingId}")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentDto>> GetByBookingId(string bookingId)
    {
        _logger.LogInformation("Getting payment for booking {BookingId}", bookingId);

        var payment = await _repository.GetByBookingIdAsync(bookingId);
        
        if (payment == null)
            return NotFound();

        return Ok(new PaymentDto(
            payment.Id,
            payment.BookingId,
            payment.Amount,
            payment.Currency,
            payment.Status.ToString(),
            payment.TransactionId
        ));
    }

    /// <summary>
    /// Calculate refund amount based on cancellation policy
    /// </summary>
    [HttpGet("{id}/refund-amount")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    public async Task<ActionResult<decimal>> CalculateRefundAmount(string id, [FromQuery] int daysBeforeTravel)
    {
        _logger.LogInformation("Calculating refund for payment {PaymentId}, days before travel: {Days}", 
            id, daysBeforeTravel);

        // Use database function to calculate refund
        var refundAmount = await _repository.CalculateRefundAmountAsync(id, daysBeforeTravel);
        
        return Ok(refundAmount);
    }

    /// <summary>
    /// Get reconciliation report for date range
    /// </summary>
    [HttpGet("reconciliation")]
    [Authorize(Roles = "TenantAdmin")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetReconciliation(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        _logger.LogInformation("Getting reconciliation report from {StartDate} to {EndDate}", 
            startDate, endDate);

        var payments = await _repository.GetByTenantAndDateAsync(
            _tenantContext.TenantId!,
            startDate,
            endDate);

        var result = payments.Select(p => new PaymentDto(
            p.Id,
            p.BookingId,
            p.Amount,
            p.Currency,
            p.Status.ToString(),
            p.TransactionId
        ));

        return Ok(new
        {
            startDate,
            endDate,
            totalPayments = result.Count(),
            totalAmount = result.Sum(p => p.Amount),
            payments = result
        });
    }
}

