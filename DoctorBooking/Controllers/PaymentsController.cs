using Application.Features.Payments.Commands.CreatePaymentIntent;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Application.Common.Interfaces;

namespace DoctorBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class PaymentsController : ControllerBase
{
    private readonly ISender              _mediator;
    private readonly StripeSettings       _stripeSettings;
    private readonly IApplicationDbContext _context;

    public PaymentsController(
        ISender                    mediator,
        IOptions<StripeSettings>   stripeSettings,
        IApplicationDbContext      context)
    {
        _mediator       = mediator;
        _stripeSettings = stripeSettings.Value;
        _context        = context;
    }

    /// <summary>
    /// Creates a Stripe PaymentIntent for a pending booking.
    /// Returns the client secret to complete payment on the frontend.
    /// </summary>
    [HttpPost("create-intent")]
    [Authorize]
    public async Task<IActionResult> CreateIntent([FromBody] CreateIntentRequest request)
    {
        var result = await _mediator.Send(new CreatePaymentIntentCommand(request.BookingId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Stripe webhook endpoint. Called by Stripe to notify about payment events.
    /// Validates the Stripe-Signature header for security.
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _stripeSettings.WebhookSecret,
                throwOnApiVersionMismatch: false);

            if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
            {
                var intent = (PaymentIntent)stripeEvent.Data.Object;
                await HandlePaymentSucceeded(intent);
            }
            else if (stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed)
            {
                var intent = (PaymentIntent)stripeEvent.Data.Object;
                await HandlePaymentFailed(intent);
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ── Private Webhook Helpers ───────────────────────────────────────────

    private async Task HandlePaymentSucceeded(PaymentIntent intent)
    {
        var payment = await _context.payments
            .Include(p => p.booking)
                .ThenInclude(b => b.availability_slot)
            .FirstOrDefaultAsync(p => p.gateway_transaction_id == intent.Id);

        if (payment is null) return;

        // Update payment record
        payment.status     = "paid";
        payment.paid_at    = DateTime.UtcNow;
        payment.updated_at = DateTime.UtcNow;
        payment.gateway_response = intent.ToJson();

        // Confirm the booking
        if (payment.booking is not null)
        {
            payment.booking.status       = "confirmed";
            payment.booking.confirmed_at = DateTime.UtcNow;
            payment.booking.updated_at   = DateTime.UtcNow;

            // Mark the slot as booked
            if (payment.booking.availability_slot is not null)
            {
                payment.booking.availability_slot.status     = "booked";
                payment.booking.availability_slot.updated_at = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task HandlePaymentFailed(PaymentIntent intent)
    {
        var payment = await _context.payments
            .Include(p => p.booking)
                .ThenInclude(b => b.availability_slot)
            .FirstOrDefaultAsync(p => p.gateway_transaction_id == intent.Id);

        if (payment is null) return;

        // Mark payment as failed
        payment.status          = "failed";
        payment.updated_at      = DateTime.UtcNow;
        payment.gateway_response = intent.ToJson();

        // Revert the booking to cancelled and release the slot
        if (payment.booking is not null)
        {
            payment.booking.status              = "cancelled";
            payment.booking.cancellation_reason = "Payment failed.";
            payment.booking.cancelled_by        = "system";
            payment.booking.cancelled_at        = DateTime.UtcNow;
            payment.booking.updated_at          = DateTime.UtcNow;

            // Release the slot back to available
            if (payment.booking.availability_slot is not null)
            {
                payment.booking.availability_slot.status     = "available";
                payment.booking.availability_slot.updated_at = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }
}

/// <summary>Input DTO for creating a PaymentIntent.</summary>
public sealed record CreateIntentRequest(long BookingId);
