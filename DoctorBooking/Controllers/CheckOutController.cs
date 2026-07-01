using System;
using System.IO;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.PaymentDTO;

namespace DoctorBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CheckOutController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public CheckOutController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("create-session")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.CreateCheckoutSessionAsync(request, userId);
            return Ok(ApiResponse<CheckoutSessionResultDto>.Ok(result, "Checkout session created successfully."));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _paymentService.ConfirmPaymentAsync(request, userId);
            return Ok(ApiResponse<PaymentConfirmResultDto>.Ok(result, "payment confirmed and booking updated successfully."));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
        }
    }

    /// <summary>
    /// Process a refund for a payment.
    /// </summary>
    [HttpPost("refund")]
    public async Task<IActionResult> RefundPayment([FromBody] RefundRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _paymentService.RefundPaymentAsync(request, userId, userRole);
            return Ok(ApiResponse<RefundResultDto>.Ok(result, "Refund processed successfully."));
        }
        catch (ArgumentException ex)
        {
            return NotFound(ApiResponse<string>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get the history of Payments (paginated and filtered based on user role).
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetPaymentHistory([FromQuery] PaymentHistoryQueryDto query)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _paymentService.GetPaymentHistoryAsync(query, userId, userRole);
            return Ok(ApiResponse<PagedResultDto<PaymentHistoryDto>>.Ok(result, "payment history retrieved successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
        }
    }

    /// <summary>
    /// Stripe webhook receiver.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"];

        if (string.IsNullOrEmpty(stripeSignature))
        {
            return BadRequest(ApiResponse<string>.Fail("Missing Stripe-Signature header."));
        }

        try
        {
            var result = await _paymentService.ProcessStripeWebhookAsync(json, stripeSignature.ToString());
            return Ok(ApiResponse<PaymentConfirmResultDto>.Ok(result, "Stripe webhook processed successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail($"Webhook error: {ex.Message}"));
        }
    }

    // ── Helpers ────────────────────────────────────────────

    private long GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return long.Parse(sub!);
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? "user";
    }
}
