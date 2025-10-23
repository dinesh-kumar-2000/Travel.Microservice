using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Entities;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Services;
using Identity.Shared;
using SharedKernel.Auditing;
using SharedKernel.Utilities;
using System.Text;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _repository;
    private readonly IPaymentGatewayFactory _gatewayFactory;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentRepository repository,
        IPaymentGatewayFactory gatewayFactory,
        ICurrentUserService currentUser,
        IAuditService auditService,
        ILogger<PaymentsController> logger)
    {
        _repository = repository;
        _gatewayFactory = gatewayFactory;
        _currentUser = currentUser;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(CreatePaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreatePaymentResponse>> Create([FromBody] CreatePaymentRequest request)
    {
        _logger.LogInformation("Creating payment for booking {BookingId} with provider {Provider}", 
            request.BookingId, request.Provider);

        try
        {
            var gateway = _gatewayFactory.GetGateway(request.Provider);

            // Create payment intent with gateway
            var intentResult = await gateway.CreatePaymentIntentAsync(
                request.Amount,
                request.Currency,
                request.CustomerEmail,
                request.ReturnUrl);

            if (!intentResult.Success || intentResult.PaymentIntentId == null)
            {
                return BadRequest(new { message = intentResult.ErrorMessage ?? "Failed to create payment intent" });
            }

            // Create payment record
            var paymentId = Guid.NewGuid().ToString();
            var payment = Payment.Create(
                paymentId,
                _currentUser.TenantId!,
                request.BookingId,
                _currentUser.UserId!,
                request.CustomerEmail,
                request.Amount,
                request.Currency,
                request.PaymentMethod,
                request.Provider
            );

            payment.SetPaymentIntent(intentResult.PaymentIntentId);
            await _repository.AddAsync(payment);

            // Audit log
            await _auditService.LogAsync(new AuditEntry
            {
                TenantId = _currentUser.TenantId!,
                UserId = _currentUser.UserId!,
                Action = "Create",
                EntityType = "Payment",
                EntityId = paymentId,
                NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.BookingId, request.Amount, request.Currency }),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
            });

            _logger.LogInformation("Payment {PaymentId} created for booking {BookingId}", paymentId, request.BookingId);

            return CreatedAtAction(nameof(GetById), new { id = paymentId }, new CreatePaymentResponse(
                paymentId,
                intentResult.ClientSecret,
                intentResult.CheckoutUrl,
                "Pending",
                "Payment created successfully"
            ));
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning(ex, "Unsupported payment provider: {Provider}", request.Provider);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment");
            return StatusCode(500, new { message = "An error occurred while creating payment" });
        }
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
        var payment = await _repository.GetByIdAsync(id);

        if (payment == null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        return Ok(ToDto(payment));
    }

    /// <summary>
    /// Get payments for a booking
    /// </summary>
    [HttpGet("booking/{bookingId}")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByBooking(string bookingId)
    {
        var payments = await _repository.GetByBookingIdAsync(bookingId);
        return Ok(payments.Select(ToDto));
    }

    /// <summary>
    /// Confirm a payment
    /// </summary>
    [HttpPost("{id}/confirm")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(ConfirmPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConfirmPaymentResponse>> Confirm(string id, [FromBody] ConfirmPaymentRequest request)
    {
        _logger.LogInformation("Confirming payment {PaymentId}", id);

        var payment = await _repository.GetByIdAsync(id);
        if (payment == null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        if (payment.Status == PaymentStatus.Completed)
        {
            return Ok(new ConfirmPaymentResponse(true, "Payment already confirmed", id, "Completed"));
        }

        try
        {
            var gateway = _gatewayFactory.GetGateway(payment.ProviderName!);
            var intentId = request.PaymentIntentId ?? payment.PaymentIntentId;

            if (string.IsNullOrEmpty(intentId))
            {
                return BadRequest(new { message = "Payment intent ID is required" });
            }

            var isConfirmed = await gateway.ConfirmPaymentAsync(intentId);

            if (isConfirmed)
            {
                payment.Complete(intentId, request.ProviderReference ?? intentId);
                await _repository.UpdateAsync(payment);

                await _auditService.LogAsync(new AuditEntry
                {
                    TenantId = payment.TenantId,
                    UserId = _currentUser.UserId!,
                    Action = "Confirm",
                    EntityType = "Payment",
                    EntityId = id,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                });

                return Ok(new ConfirmPaymentResponse(true, "Payment confirmed successfully", id, "Completed"));
            }

            return BadRequest(new { message = "Payment confirmation failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm payment {PaymentId}", id);
            return StatusCode(500, new { message = "An error occurred while confirming payment" });
        }
    }

    /// <summary>
    /// Refund a payment
    /// </summary>
    [HttpPost("{id}/refund")]
    [Authorize(Roles = "TenantAdmin,SuperAdmin")]
    [EnableRateLimiting("fixed")]
    [ProducesResponseType(typeof(RefundPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RefundPaymentResponse>> Refund(string id, [FromBody] RefundPaymentRequest request)
    {
        _logger.LogInformation("Refunding payment {PaymentId}", id);

        var payment = await _repository.GetByIdAsync(id);
        if (payment == null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            return BadRequest(new { message = "Only completed payments can be refunded" });
        }

        try
        {
            var gateway = _gatewayFactory.GetGateway(payment.ProviderName!);
            var refundAmount = request.Amount ?? payment.Amount;

            var refundResult = await gateway.RefundPaymentAsync(
                payment.PaymentIntentId ?? payment.TransactionId!,
                refundAmount,
                request.Reason);

            if (refundResult.Success)
            {
                payment.Refund(refundAmount, request.Reason);
                await _repository.UpdateAsync(payment);

                await _auditService.LogAsync(new AuditEntry
                {
                    TenantId = payment.TenantId,
                    UserId = _currentUser.UserId!,
                    Action = "Refund",
                    EntityType = "Payment",
                    EntityId = id,
                    NewValues = System.Text.Json.JsonSerializer.Serialize(new { refundAmount, request.Reason }),
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                });

                return Ok(new RefundPaymentResponse(
                    true,
                    "Refund processed successfully",
                    refundResult.RefundId!,
                    refundAmount,
                    "Refunded"
                ));
            }

            return BadRequest(new { message = refundResult.ErrorMessage ?? "Refund failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refund payment {PaymentId}", id);
            return StatusCode(500, new { message = "An error occurred while processing refund" });
        }
    }

    /// <summary>
    /// Webhook handler for Stripe
    /// </summary>
    [HttpPost("webhooks/stripe")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body, Encoding.UTF8).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        _logger.LogInformation("Received Stripe webhook");

        try
        {
            var gateway = _gatewayFactory.GetGateway("stripe");
            var isValid = await gateway.VerifyWebhookSignatureAsync(json, signature);

            if (!isValid)
            {
                _logger.LogWarning("Invalid Stripe webhook signature");
                return BadRequest();
            }

            // Process webhook event here
            _logger.LogInformation("Stripe webhook processed successfully");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Stripe webhook");
            return StatusCode(500);
        }
    }

    /// <summary>
    /// Webhook handler for Razorpay
    /// </summary>
    [HttpPost("webhooks/razorpay")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> RazorpayWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body, Encoding.UTF8).ReadToEndAsync();
        var signature = Request.Headers["X-Razorpay-Signature"].ToString();

        _logger.LogInformation("Received Razorpay webhook");

        try
        {
            var gateway = _gatewayFactory.GetGateway("razorpay");
            var isValid = await gateway.VerifyWebhookSignatureAsync(json, signature);

            if (!isValid)
            {
                _logger.LogWarning("Invalid Razorpay webhook signature");
                return BadRequest();
            }

            // Process webhook event here
            _logger.LogInformation("Razorpay webhook processed successfully");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Razorpay webhook");
            return StatusCode(500);
        }
    }

    private static PaymentDto ToDto(Payment payment) => new(
        payment.Id,
        payment.TenantId,
        payment.BookingId,
        payment.Amount,
        payment.Currency,
        payment.PaymentMethod,
        payment.Status.ToString(),
        payment.ProviderName,
        payment.TransactionId,
        payment.ProviderReference,
        payment.CustomerId,
        payment.CustomerEmail,
        payment.CompletedAt,
        payment.CreatedAt
    );
}
