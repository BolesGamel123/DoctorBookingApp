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
public class UsersController : ControllerBase
{
    private readonly AppDBContext _context;

    public UsersController(AppDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get current authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetCurrentUserId();
        var existingUser = await _context.users
            .FirstOrDefaultAsync(u => u.id == userId && u.deleted_at == null);

        if (existingUser == null)
            return NotFound(ApiResponse<string>.Fail("user not found."));

        return Ok(ApiResponse<UserDto>.Ok(MapToDto(existingUser)));
    }

    /// <summary>
    /// Update current authenticated user's profile.
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request)
    {
        var userId = GetCurrentUserId();
        var existingUser = await _context.users
            .FirstOrDefaultAsync(u => u.id == userId && u.deleted_at == null);

        if (existingUser == null)
            return NotFound(ApiResponse<string>.Fail("user not found."));

        if (request.FullName is not null) existingUser.full_name = request.FullName;
        if (request.MobileNumber is not null) existingUser.mobile_number = request.MobileNumber;
        if (request.Birthdate is not null) existingUser.birthdate = request.Birthdate;
        if (request.ProfilePhotoUrl is not null) existingUser.profile_photo_url = request.ProfilePhotoUrl;
        if (request.LocationLabel is not null) existingUser.location_label = request.LocationLabel;
        if (request.Latitude is not null) existingUser.latitude = request.Latitude;
        if (request.Longitude is not null) existingUser.longitude = request.Longitude;

        existingUser.updated_at = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<UserDto>.Ok(MapToDto(existingUser), "Profile updated."));
    }

    /// <summary>
    /// Get all users (admin only, paginated).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _context.users.Where(u => u.deleted_at == null);

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.created_at)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => MapToDto(u))
            .ToListAsync();

        var result = new { TotalCount = totalCount, Page = page, PageSize = pageSize, Users = users };
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Get a user by ID (admin only).
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetById(long id)
    {
        var existingUser = await _context.users
            .FirstOrDefaultAsync(u => u.id == id && u.deleted_at == null);

        if (existingUser == null)
            return NotFound(ApiResponse<string>.Fail("user not found."));

        return Ok(ApiResponse<UserDto>.Ok(MapToDto(existingUser)));
    }

    /// <summary>
    /// Soft-delete a user (admin only).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(long id)
    {
        var existingUser = await _context.users.FindAsync(id);
        if (existingUser == null || existingUser.deleted_at != null)
            return NotFound(ApiResponse<string>.Fail("user not found."));

        existingUser.deleted_at = DateTime.UtcNow;
        existingUser.is_active = false;
        existingUser.refresh_token = null;
        existingUser.refresh_token_expiry = null;
        existingUser.updated_at = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<string>.Ok("user deleted.", "user soft-deleted successfully."));
    }

    // ── Helpers ────────────────────────────────────────────

    private long GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return long.Parse(sub!);
    }

    private static UserDto MapToDto(user u) => new()
    {
        Id = u.id,
        FullName = u.full_name,
        Email = u.email,
        MobileNumber = u.mobile_number,
        ProfilePhotoUrl = u.profile_photo_url,
        Birthdate = u.birthdate,
        IsActive = u.is_active,
        CreatedAt = u.created_at
    };
}
