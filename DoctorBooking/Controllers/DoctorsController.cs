using Application.Features.Doctors.Queries.GetNearbyDoctors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Patient")]
public sealed class DoctorsController : ControllerBase
{
    private readonly ISender _mediator;

    public DoctorsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get nearby doctors based on patient's current latitude and longitude.
    /// </summary>
    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearbyDoctors(
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        [FromQuery] double radiusKm = 10,
        [FromQuery] int limit = 20)
    {
        var result = await _mediator.Send(
            new GetNearbyDoctorsQuery(latitude, longitude, radiusKm, limit));

        return result.Success ? Ok(result) : BadRequest(result);
    }
}