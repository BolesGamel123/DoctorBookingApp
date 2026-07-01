using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.PaymentDTO;
using Application.Interfaces;
using Domain.Models;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDBContext _context;
    private readonly StripeSettings _stripeSettings;

    public PaymentService(AppDBContext context, IOptions<StripeSettings> stripeSettings)
    {
        _context = context;
        _stripeSettings = stripeSettings.Value;
    }

    public async Task<CheckoutSessionResultDto> CreateCheckoutSessionAsync(CreateCheckoutSessionRequestDto request, long currentUserId)
    {
        var booking = await _context.bookings
            .Include(b => b.doctor)
            .Include(b => b.user)
            .FirstOrDefaultAsync(b => b.id == request.BookingId);

        if (booking == null)
            throw new ArgumentException("booking not found.");

        if (booking.user_id != currentUserId)
            throw new UnauthorizedAccessException("You are not authorized to pay for this booking.");

        if (booking.status != "pending")
            throw new InvalidOperationException($"Cannot pay for booking in '{booking.status}' status.");

        if (booking.session_price <= 0)
            throw new InvalidOperationException("Invalid booking price.");

        
        var existingPayment = await _context.payments
            .FirstOrDefaultAsync(p => p.booking_id == booking.id && p.status == "completed");
        if (existingPayment != null)
            throw new InvalidOperationException("This booking has already been paid for.");

        if (request.UseStripe)
        { 
            var currencyCode = string.IsNullOrWhiteSpace(booking.currency) ? "usd" : booking.currency.ToLower();
            
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(booking.session_price * 100), 
                            Currency = currencyCode,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"doctor Consultation - Dr. {booking.doctor.full_name}",
                                Description = $"Session scheduled on {booking.booked_date} at {booking.booked_start_time}",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = request.SuccessUrl + "?session_id={CHECKOUT_SESSION_ID}&booking_id=" + booking.id,
                CancelUrl = request.CancelUrl,
                CustomerEmail = booking.user?.email,
                Metadata = new Dictionary<string, string>
                {
                    { "BookingId", booking.id.ToString() },
                    { "UserId", currentUserId.ToString() },
                    { "DoctorId", booking.doctor_id.ToString() }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return new CheckoutSessionResultDto
            {
                SessionId = session.Id,
                CheckoutUrl = session.Url,
                PaymentIntentId = session.PaymentIntentId ?? string.Empty,
                ClientSecret = session.ClientSecret ?? string.Empty,
                IsStripe = true
            };
        }
        else
        {
            
            var sessionId = "mock_sess_" + Guid.NewGuid().ToString("N");
            var checkoutUrl = $"http://localhost:5000/mock-checkout?session_id={sessionId}&booking_id={booking.id}&amount={booking.session_price}&currency={booking.currency}";

            return new CheckoutSessionResultDto
            {
                SessionId = sessionId,
                CheckoutUrl = checkoutUrl,
                PaymentIntentId = "mock_pi_" + Guid.NewGuid().ToString("N").Substring(0, 16),
                ClientSecret = "mock_cs_" + Guid.NewGuid().ToString("N"),
                IsStripe = false
            };
        }
    }

    public async Task<PaymentConfirmResultDto> ConfirmPaymentAsync(ConfirmPaymentRequestDto request, long currentUserId)
    {
        
        var booking = await _context.bookings
            .Include(b => b.doctor)
            .FirstOrDefaultAsync(b => b.id == request.BookingId);

        if (booking == null)
            throw new ArgumentException("booking not found.");

        if (booking.user_id != currentUserId)
            throw new UnauthorizedAccessException("You are not authorized to pay for this booking.");

        if (booking.status != "pending")
            throw new InvalidOperationException($"booking status is '{booking.status}'. Only pending Bookings can be confirmed.");

        var existingPayment = await _context.payments
            .FirstOrDefaultAsync(p => p.booking_id == booking.id && p.status == "completed");
        if (existingPayment != null)
            throw new InvalidOperationException("This booking has already been paid for.");

        string gatewayTransactionId = request.GatewayTransactionId;
        string gatewayResponse = request.GatewayResponse;
        string gatewayName = "Mock";

        
        if (request.GatewayTransactionId.StartsWith("cs_")) 
        {
            gatewayName = "Stripe";
            var service = new SessionService();
            Session session = await service.GetAsync(request.GatewayTransactionId);
            
            if (session.PaymentStatus != "paid")
                throw new InvalidOperationException("The Stripe checkout session has not been paid yet.");

            gatewayTransactionId = session.PaymentIntentId ?? session.Id;
            gatewayResponse = System.Text.Json.JsonSerializer.Serialize(session);
        }
        else if (request.GatewayTransactionId.StartsWith("pi_")) 
        {
            gatewayName = "Stripe";
            var service = new PaymentIntentService();
            PaymentIntent pi = await service.GetAsync(request.GatewayTransactionId);

            if (pi.Status != "succeeded")
                throw new InvalidOperationException($"The Stripe payment intent status is '{pi.Status}', not succeeded.");

            gatewayTransactionId = pi.Id;
            gatewayResponse = System.Text.Json.JsonSerializer.Serialize(pi);
        }

        // 3. Update the associated Availability Slot
        var slot = await _context.doctor_availability_slots
            .FirstOrDefaultAsync(s => s.id == booking.availability_slot_id);
        
        if (slot != null)
        {
            slot.status = "booked";
            slot.booking_id = booking.id;
            slot.updated_at = DateTime.UtcNow;
        }

        
        decimal platformFee = booking.session_price * 0.10M; 
        decimal doctorEarnings = booking.session_price - platformFee;

       
        var payment = new payment
        {
            booking_id = booking.id,
            user_id = booking.user_id,
            doctor_id = booking.doctor_id,
            payment_method = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "Card" : request.PaymentMethod,
            gateway = gatewayName,
            gateway_transaction_id = gatewayTransactionId,
            gateway_response = gatewayResponse,
            amount = booking.session_price,
            currency = string.IsNullOrWhiteSpace(booking.currency) ? "USD" : booking.currency,
            platform_fee = platformFee,
            doctor_earnings = doctorEarnings,
            status = "completed",
            refunded_amount = 0M,
            paid_at = DateTime.UtcNow,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        
        booking.status = "confirmed";
        booking.confirmed_at = DateTime.UtcNow;
        booking.updated_at = DateTime.UtcNow;

        _context.payments.Add(payment);
        await _context.SaveChangesAsync();

        return new PaymentConfirmResultDto
        {
            PaymentId = payment.id,
            BookingId = booking.id,
            Status = payment.status,
            Amount = payment.amount,
            PaidAt = payment.paid_at
        };
    }

    public async Task<PaymentConfirmResultDto> ProcessStripeWebhookAsync(string jsonPayload, string stripeSignatureHeader)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                jsonPayload, 
                stripeSignatureHeader, 
                _stripeSettings.WebhookSecret,
                throwOnApiVersionMismatch: false
            );
        }
        catch (Exception ex)
        {
            throw new Exception($"Webhook verification failed: {ex.Message}");
        }

        if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = stripeEvent.Data.Object as Session;
            if (session != null && session.PaymentStatus == "paid")
            {
                if (session.Metadata != null && session.Metadata.TryGetValue("BookingId", out var bookingIdStr) && long.TryParse(bookingIdStr, out var bookingId))
                {
                    // Check if already paid to avoid double processing
                    var booking = await _context.bookings.FindAsync(bookingId);
                    if (booking != null && booking.status == "pending")
                    {
                        var userIdStr = session.Metadata["UserId"];
                        long.TryParse(userIdStr, out var userId);

                        var confirmRequest = new ConfirmPaymentRequestDto
                        {
                            BookingId = bookingId,
                            PaymentMethod = "StripeCard",
                            GatewayTransactionId = session.Id,
                            GatewayResponse = jsonPayload
                        };

                        return await ConfirmPaymentAsync(confirmRequest, userId);
                    }
                }
            }
        }
        else if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var pi = stripeEvent.Data.Object as PaymentIntent;
            if (pi != null)
            {
                if (pi.Metadata != null && pi.Metadata.TryGetValue("BookingId", out var bookingIdStr) && long.TryParse(bookingIdStr, out var bookingId))
                {
                    var booking = await _context.bookings.FindAsync(bookingId);
                    if (booking != null && booking.status == "pending")
                    {
                        var userIdStr = pi.Metadata["UserId"];
                        long.TryParse(userIdStr, out var userId);

                        var confirmRequest = new ConfirmPaymentRequestDto
                        {
                            BookingId = bookingId,
                            PaymentMethod = "StripeCard",
                            GatewayTransactionId = pi.Id,
                            GatewayResponse = jsonPayload
                        };

                        return await ConfirmPaymentAsync(confirmRequest, userId);
                    }
                }
            }
        }

        return new PaymentConfirmResultDto { Status = "ignored" };
    }

    public async Task<RefundResultDto> RefundPaymentAsync(RefundRequestDto request, long currentUserId, string currentUserRole)
    {
        // 1. Fetch the payment
        var payment = await _context.payments
            .Include(p => p.booking)
            .FirstOrDefaultAsync(p => p.id == request.PaymentId);

        if (payment == null)
            throw new ArgumentException("payment record not found.");

        if (payment.status != "completed")
            throw new InvalidOperationException($"Cannot refund payment with status '{payment.status}'. Only completed Payments can be refunded.");

        // 2. Authorization Checks
        if (currentUserRole == "user" && payment.user_id != currentUserId)
            throw new UnauthorizedAccessException("You are not authorized to request a refund for this payment.");

        if (currentUserRole == "doctor" && payment.doctor_id != currentUserId)
            throw new UnauthorizedAccessException("You are not authorized to issue a refund for this payment.");

        // 3. Process Gateway Refund
        decimal refundAmount = request.Amount ?? payment.amount;
        if (refundAmount <= 0 || refundAmount > payment.amount)
            throw new ArgumentException($"Invalid refund amount. Must be between 0 and {payment.amount}.");

        if (payment.gateway == "Stripe")
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = payment.gateway_transaction_id,
                    Amount = (long)(refundAmount * 100), // convert to cents
                    Reason = RefundReasons.RequestedByCustomer
                };

                var service = new RefundService();
                Refund refund = await service.CreateAsync(options);
                payment.gateway_response += "\nRefundResponse: " + System.Text.Json.JsonSerializer.Serialize(refund);
            }
            catch (Exception ex)
            {
                throw new Exception($"Stripe Refund failed: {ex.Message}");
            }
        }

        // 4. Update payment status
        payment.status = "refunded";
        payment.refunded_amount = refundAmount;
        payment.refund_reason = string.IsNullOrWhiteSpace(request.RefundReason) ? "Cancelled by user" : request.RefundReason;
        payment.refunded_at = DateTime.UtcNow;
        payment.updated_at = DateTime.UtcNow;

        // 5. Update booking Status
        if (payment.booking != null)
        {
            payment.booking.status = "cancelled";
            payment.booking.cancelled_by = currentUserRole;
            payment.booking.cancelled_at = DateTime.UtcNow;
            payment.booking.cancellation_reason = payment.refund_reason;
            payment.booking.updated_at = DateTime.UtcNow;

            // 6. Free up Availability Slot
            var slot = await _context.doctor_availability_slots
                .FirstOrDefaultAsync(s => s.id == payment.booking.availability_slot_id);
            if (slot != null)
            {
                slot.status = "available";
                slot.booking_id = null;
                slot.updated_at = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return new RefundResultDto
        {
            PaymentId = payment.id,
            RefundedAmount = refundAmount,
            Status = payment.status,
            RefundedAt = payment.refunded_at.Value
        };
    }

    public async Task<PagedResultDto<PaymentHistoryDto>> GetPaymentHistoryAsync(PaymentHistoryQueryDto query, long currentUserId, string currentUserRole)
    {
        var dbQuery = _context.payments
            .Include(p => p.booking)
            .Include(p => p.user)
            .Include(p => p.doctor)
            .AsQueryable();

        // 1. Role Filtering
        if (currentUserRole == "user")
        {
            dbQuery = dbQuery.Where(p => p.user_id == currentUserId);
        }
        else if (currentUserRole == "doctor")
        {
            dbQuery = dbQuery.Where(p => p.doctor_id == currentUserId);
        }
        else if (currentUserRole == "admin")
        {
            // Admins can see everything
        }
        else
        {
            throw new UnauthorizedAccessException("Invalid role access.");
        }

        // 2. Status Filtering
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            dbQuery = dbQuery.Where(p => p.status == query.Status);
        }

        // 3. Count
        int totalCount = await dbQuery.CountAsync();

        // 4. Pagination
        var payments = await dbQuery
            .OrderByDescending(p => p.created_at)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new PaymentHistoryDto
            {
                PaymentId = p.id,
                BookingId = p.booking_id,
                PatientId = p.user_id,
                PatientName = p.user != null ? p.user.full_name : "Unknown Patient",
                DoctorId = p.doctor_id,
                DoctorName = p.doctor != null ? p.doctor.full_name : "Unknown doctor",
                Amount = p.amount,
                Currency = p.currency,
                Status = p.status,
                PaidAt = p.paid_at,
                PlatformFee = p.platform_fee,
                DoctorEarnings = p.doctor_earnings,
                CreatedAt = p.created_at
            })
            .ToListAsync();

        return new PagedResultDto<PaymentHistoryDto>
        {
            Items = payments,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
}
