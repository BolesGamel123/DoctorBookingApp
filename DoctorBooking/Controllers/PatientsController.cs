using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.DTOs;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly AppDBContext _context;

    public PatientsController(AppDBContext context)
    {
        _context = context;
    }

    /// <summary>Get current authenticated patient's profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var patientId = GetCurrentUserId();
        var patient = await _context.patients
            .FirstOrDefaultAsync(u => u.id == patientId && u.deleted_at == null);

        if (patient == null)
            return NotFound(ApiResponse<string>.Fail("Patient not found."));

        return Ok(ApiResponse<PatientDto>.Ok(MapToDto(patient)));
    }

    /// <summary>Update current authenticated patient's profile.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdatePatientRequest request)
    {
        var patientId = GetCurrentUserId();
        var patient = await _context.patients
            .FirstOrDefaultAsync(u => u.id == patientId && u.deleted_at == null);

        if (patient == null)
            return NotFound(ApiResponse<string>.Fail("Patient not found."));

        if (request.FullName is not null)       patient.full_name       = request.FullName;
        if (request.MobileNumber is not null)   patient.mobile_number   = request.MobileNumber;
        if (request.Birthdate is not null)      patient.birthdate       = request.Birthdate;
        if (request.ProfilePhotoUrl is not null) patient.profile_photo_url = request.ProfilePhotoUrl;
        if (request.LocationLabel is not null)  patient.location_label  = request.LocationLabel;
        if (request.Latitude is not null)       patient.latitude        = request.Latitude;
        if (request.Longitude is not null)      patient.longitude       = request.Longitude;

        patient.updated_at = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<PatientDto>.Ok(MapToDto(patient), "Profile updated."));
    }

    /// <summary>Get all patients (admin only, paginated).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _context.patients.Where(u => u.deleted_at == null);

        var totalCount = await query.CountAsync();
        var patients = await query
            .OrderByDescending(u => u.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => MapToDto(u))
            .ToListAsync();

        var result = new { TotalCount = totalCount, Page = page, PageSize = pageSize, Patients = patients };
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Get a patient by ID (admin only).</summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(long id)
    {
        var patient = await _context.patients
            .FirstOrDefaultAsync(u => u.id == id && u.deleted_at == null);

        if (patient == null)
            return NotFound(ApiResponse<string>.Fail("Patient not found."));

        return Ok(ApiResponse<PatientDto>.Ok(MapToDto(patient)));
    }

    /// <summary>Soft-delete a patient (admin only).</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        var patient = await _context.patients.FindAsync(id);
        if (patient == null || patient.deleted_at != null)
            return NotFound(ApiResponse<string>.Fail("Patient not found."));

        patient.deleted_at           = DateTime.UtcNow;
        patient.is_active            = false;
        patient.refresh_token        = null;
        patient.refresh_token_expiry = null;
        patient.updated_at           = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("Patient deleted.", "Patient soft-deleted successfully."));
    }

    // ── Helpers ────────────────────────────────────────────

    private long GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return long.Parse(sub!);
    }

    private static PatientDto MapToDto(patient u) => new()
    {
        Id              = u.id,
        FullName        = u.full_name,
        Email           = u.email,
        MobileNumber    = u.mobile_number,
        ProfilePhotoUrl = u.profile_photo_url,
        Birthdate       = u.birthdate,
        IsActive        = u.is_active,
        CreatedAt       = u.created_at
    };
}
