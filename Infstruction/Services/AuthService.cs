using Application.DTOs;
using Application.Interfaces;
using Domain.Models;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDBContext _context;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDBContext context, ITokenService tokenService, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        // Validate passwords match
        if (request.Password != request.ConfirmPassword)
            return AuthResult.Failure("Passwords do not match.");

        // Check if email already exists
        if (await _context.users.AnyAsync(u => u.email == request.Email && u.deleted_at == null))
            return AuthResult.Failure("Email is already registered.");

        // Check if mobile already exists
        if (await _context.users.AnyAsync(u => u.mobile_number == request.MobileNumber && u.deleted_at == null))
            return AuthResult.Failure("Mobile number is already registered.");

        // Create User
        var newUser = new User
        {
            full_name = request.FullName,
            email = request.Email,
            mobile_number = request.MobileNumber,
            password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            is_active = true,
            is_banned = false,
            remember_me = false,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        _context.users.Add(newUser);
        await _context.SaveChangesAsync();

        // Generate tokens
        var roles = new List<string> { "User" };
        var accessToken = _tokenService.GenerateAccessToken(newUser.id, newUser.email, newUser.full_name, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        newUser.refresh_token = refreshToken;
        newUser.refresh_token_expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _context.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        return AuthResult.Success(accessToken, refreshToken, expiresAt, MapToDto(newUser));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var existingUser = await _context.users
            .FirstOrDefaultAsync(u => u.email == request.Email && u.deleted_at == null);

        if (existingUser == null)
            return AuthResult.Failure("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, existingUser.password))
            return AuthResult.Failure("Invalid email or password.");

        if (existingUser.is_banned)
            return AuthResult.Failure("Your account has been suspended.", existingUser.ban_reason ?? "Contact support.");

        if (!existingUser.is_active)
            return AuthResult.Failure("Your account is not active.");

        // Generate tokens
        var roles = new List<string> { "User" };
        var accessToken = _tokenService.GenerateAccessToken(existingUser.id, existingUser.email, existingUser.full_name, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        existingUser.refresh_token = refreshToken;
        existingUser.refresh_token_expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        existingUser.updated_at = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        return AuthResult.Success(accessToken, refreshToken, expiresAt, MapToDto(existingUser));
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var existingUser = await _context.users
            .FirstOrDefaultAsync(u => u.refresh_token == refreshToken && u.deleted_at == null);

        if (existingUser == null || existingUser.refresh_token_expiry < DateTime.UtcNow)
            return AuthResult.Failure("Invalid or expired refresh token.");

        // Rotate tokens
        var roles = new List<string> { "User" };
        var newAccessToken = _tokenService.GenerateAccessToken(existingUser.id, existingUser.email, existingUser.full_name, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        existingUser.refresh_token = newRefreshToken;
        existingUser.refresh_token_expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        existingUser.updated_at = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        return AuthResult.Success(newAccessToken, newRefreshToken, expiresAt, MapToDto(existingUser));
    }

    public async Task<bool> RevokeTokenAsync(long userId)
    {
        var existingUser = await _context.users.FindAsync(userId);
        if (existingUser == null) return false;

        existingUser.refresh_token = null;
        existingUser.refresh_token_expiry = null;
        existingUser.updated_at = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Helper ─────────────────────────────────────────────

    private static UserDto MapToDto(User u) => new()
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
