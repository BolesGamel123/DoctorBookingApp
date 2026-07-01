using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new User account.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<AuthResult>.Fail("Registration failed.", result.Errors));

        return Ok(ApiResponse<AuthResult>.Ok(result, "Registration successful."));
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Succeeded)
            return Unauthorized(ApiResponse<AuthResult>.Fail("Login failed.", result.Errors));

        return Ok(ApiResponse<AuthResult>.Ok(result, "Login successful."));
    }

    /// <summary>
    /// Refresh access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Succeeded)
            return Unauthorized(ApiResponse<AuthResult>.Fail("Token refresh failed.", result.Errors));

        return Ok(ApiResponse<AuthResult>.Ok(result, "Token refreshed."));
    }

    /// <summary>
    /// Revoke refresh token (logout).
    /// </summary>
    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke()
    {
        var userId = long.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var revoked = await _authService.RevokeTokenAsync(userId);

        if (!revoked)
            return NotFound(ApiResponse<string>.Fail("User not found."));

        return Ok(ApiResponse<string>.Ok("Refresh token revoked.", "Logged out successfully."));
    }
}
