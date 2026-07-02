using Application.Common.Interfaces;
using Application.DTOs;
using Application.Features.Doctors.Queries.GetNearbyDoctors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace DoctorBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Patient")]
public sealed class DoctorsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ISearchHistoryService service;

    public DoctorsController(ISender mediator,
                             ISearchHistoryService service)
    {
        _mediator = mediator;
        this.service = service;
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


    [HttpGet("search")]
    [ProducesResponseType(typeof(GeneicResult<DoctorSearchResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeneicResult<DoctorSearchResponseDTO>>> SearchDoctors([FromQuery] DoctorSearchDTO dto)
    {
        var result = await service.GetDoctor(dto);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}