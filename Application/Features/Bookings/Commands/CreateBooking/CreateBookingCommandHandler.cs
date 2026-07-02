using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Bookings.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandHandler
    : IRequestHandler<CreateBookingCommand, ApiResponse<BookingResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService   _currentUser;

    public CreateBookingCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService   currentUser)
    {
        _context     = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<BookingResponse>> Handle(
        CreateBookingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return ApiResponse<BookingResponse>.Fail("Unauthorized.");

        var userId = _currentUser.UserId.Value;

        // 1. Load the slot and eagerly include the doctor
        var slot = await _context.doctor_availability_slots
            .Include(s => s.doctor)
            .FirstOrDefaultAsync(s => s.id == request.SlotId, cancellationToken);

        if (slot is null)
            return ApiResponse<BookingResponse>.Fail("Slot not found.");

        if (slot.status != "available")
            return ApiResponse<BookingResponse>.Fail("The selected time slot is no longer available.");

        // 2. Reserve the slot so no one else can grab it while the patient pays
        slot.status     = "reserved";
        slot.updated_at = DateTime.UtcNow;

        // 3. Build a unique booking reference  e.g. BK-20260701-A3F9
        var reference = $"BK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpperInvariant()}";

        // 4. Create the booking
        var newBooking = new booking
        {
            booking_reference   = reference,
            user_id             = userId,
            doctor_id           = slot.doctor_id,
            availability_slot_id = slot.id,
            booked_date         = slot.slot_date,
            booked_start_time   = slot.slot_start_time,
            booked_end_time     = slot.slot_end_time,
            status              = "pending",
            payment_method      = request.PaymentMethod,
            session_price       = slot.doctor.session_price,
            currency            = slot.doctor.currency,
            notes_for_doctor    = request.NotesForDoctor,
            is_rebooking        = false,
            created_at          = DateTime.UtcNow,
            updated_at          = DateTime.UtcNow
        };

        _context.bookings.Add(newBooking);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new BookingResponse
        {
            Id               = newBooking.id,
            BookingReference = newBooking.booking_reference,
            SlotId           = newBooking.availability_slot_id,
            DoctorId         = newBooking.doctor_id,
            DoctorName       = slot.doctor.full_name,
            Status           = newBooking.status,
            PaymentMethod    = newBooking.payment_method,
            SessionPrice     = newBooking.session_price,
            Currency         = newBooking.currency,
            BookedDate       = newBooking.booked_date,
            StartTime        = newBooking.booked_start_time,
            EndTime          = newBooking.booked_end_time,
            NotesForDoctor   = newBooking.notes_for_doctor,
            CreatedAt        = newBooking.created_at
        };

        return ApiResponse<BookingResponse>.Ok(response, "Booking created. Proceed to payment.");
    }
}
