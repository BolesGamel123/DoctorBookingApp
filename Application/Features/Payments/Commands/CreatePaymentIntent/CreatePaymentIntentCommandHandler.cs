using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Payments.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Features.Payments.Commands.CreatePaymentIntent;

public sealed class CreatePaymentIntentCommandHandler
    : IRequestHandler<CreatePaymentIntentCommand, ApiResponse<PaymentIntentResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStripeService        _stripe;
    private readonly ICurrentUserService   _currentUser;
    private readonly StripePublicSettings  _stripePublic;

    public CreatePaymentIntentCommandHandler(
        IApplicationDbContext      context,
        IStripeService             stripe,
        ICurrentUserService        currentUser,
        IOptions<StripePublicSettings> stripePublic)
    {
        _context      = context;
        _stripe       = stripe;
        _currentUser  = currentUser;
        _stripePublic = stripePublic.Value;
    }

    public async Task<ApiResponse<PaymentIntentResponse>> Handle(
        CreatePaymentIntentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return ApiResponse<PaymentIntentResponse>.Fail("Unauthorized.");

        var userId = _currentUser.UserId.Value;

        // 1. Load the booking
        var booking = await _context.bookings
            .FirstOrDefaultAsync(b => b.id == request.BookingId && b.user_id == userId, cancellationToken);

        if (booking is null)
            return ApiResponse<PaymentIntentResponse>.Fail("Booking not found.");

        if (booking.status != "pending")
            return ApiResponse<PaymentIntentResponse>.Fail(
                $"Cannot create payment for a booking with status '{booking.status}'.");

        // 2. Check if payment already exists for this booking
        var existingPayment = await _context.payments
            .FirstOrDefaultAsync(p => p.booking_id == booking.id, cancellationToken);

        if (existingPayment is not null && existingPayment.status == "paid")
            return ApiResponse<PaymentIntentResponse>.Fail("This booking has already been paid.");

        // 3. Create PaymentIntent on Stripe
        var (clientSecret, intentId) = await _stripe.CreatePaymentIntentAsync(
            booking.session_price,
            booking.currency,
            booking.booking_reference,
            cancellationToken);

        // 4. Upsert the payment record
        if (existingPayment is null)
        {
            var newPayment = new payment
            {
                booking_id             = booking.id,
                user_id                = booking.user_id,
                doctor_id              = booking.doctor_id,
                payment_method         = booking.payment_method,
                gateway                = "stripe",
                gateway_transaction_id = intentId,
                amount                 = booking.session_price,
                currency               = booking.currency,
                platform_fee           = 0,
                doctor_earnings        = booking.session_price,
                status                 = "pending",
                refunded_amount        = 0,
                created_at             = DateTime.UtcNow,
                updated_at             = DateTime.UtcNow
            };
            _context.payments.Add(newPayment);
        }
        else
        {
            existingPayment.gateway_transaction_id = intentId;
            existingPayment.status                 = "pending";
            existingPayment.updated_at             = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var result = new PaymentIntentResponse
        {
            ClientSecret     = clientSecret,
            PublishableKey   = _stripePublic.PublishableKey,
            PaymentIntentId  = intentId,
            Amount           = booking.session_price,
            Currency         = booking.currency,
            BookingReference = booking.booking_reference
        };

        return ApiResponse<PaymentIntentResponse>.Ok(result, "PaymentIntent created successfully.");
    }
}
