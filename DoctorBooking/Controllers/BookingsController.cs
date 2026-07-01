using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Features.Bookings.Commands.CreateBooking;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class BookingsController : ControllerBase
{
    private readonly ISender _mediator;

    public BookingsController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// Create a new booking for an available doctor slot.
    /// Returns a pending booking. Call POST /api/payments/create-intent next to pay.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var command = new CreateBookingCommand(
            request.SlotId,
            request.PaymentMethod,
            request.NotesForDoctor);

        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

/// <summary>Input DTO for creating a booking.</summary>
public sealed record CreateBookingRequest(
    long    SlotId,
    string  PaymentMethod   = "card",
    string? NotesForDoctor  = null);
