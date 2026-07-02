using Application.Common.Models;
using Application.Features.Bookings.DTOs;
using MediatR;

namespace Application.Features.Bookings.Commands.CreateBooking;

/// <summary>
/// Command to create a new booking for an available doctor slot.
/// </summary>
public sealed record CreateBookingCommand(
    long    SlotId,
    string  PaymentMethod,
    string? NotesForDoctor = null
) : IRequest<ApiResponse<BookingResponse>>;
