using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Features.Patients.Commands.DeletePatient;
using Application.Features.Patients.Commands.UpdatePatient;
using Application.Features.Patients.Queries.GetAllPatients;
using Application.Features.Patients.Queries.GetCurrentPatient;
using Application.Features.Patients.Queries.GetPatientById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class PatientsController : ControllerBase
{
    private readonly ISender _mediator;

    public PatientsController(ISender mediator) => _mediator = mediator;

    /// <summary>Get current authenticated patient's profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new GetCurrentPatientQuery(userId));
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Update current authenticated patient's profile.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdatePatientRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdatePatientCommand(
            userId,
            request.FullName, request.MobileNumber, request.Birthdate,
            request.ProfilePhotoUrl, request.LocationLabel,
            request.Latitude, request.Longitude);

        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get all patients (admin only, paginated).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllPatientsQuery(page, pageSize));
        return Ok(result);
    }

    /// <summary>Get a patient by ID (admin only).</summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetPatientByIdQuery(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Soft-delete a patient (admin only).</summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeletePatientCommand(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    private long GetCurrentUserId()
        => long.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}

/// <summary>Input DTO for patient profile updates.</summary>
public sealed record UpdatePatientRequest(
    string? FullName = null,
    string? MobileNumber = null,
    DateOnly? Birthdate = null,
    string? ProfilePhotoUrl = null,
    string? LocationLabel = null,
    decimal? Latitude = null,
    decimal? Longitude = null);
