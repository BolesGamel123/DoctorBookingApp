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
        if (request.Password != request.ConfirmPassword)
            return AuthResult.Failure("Passwords do not match.");

        if (await _context.users.AnyAsync(u => u.email == request.Email && u.deleted_at == null))
            return AuthResult.Failure("Email is already registered.");

        if (await _context.users.AnyAsync(u => u.mobile_number == request.MobileNumber && u.deleted_at == null))
            return AuthResult.Failure("Mobile number is already registered.");

        var newPatient = new user
        {
            full_name     = request.FullName,
            email         = request.Email,
            mobile_number = request.MobileNumber,
            password      = BCrypt.Net.BCrypt.HashPassword(request.Password),
            is_active     = true,
            is_banned     = false,
            remember_me   = false,
            created_at    = DateTime.UtcNow,
            updated_at    = DateTime.UtcNow
        };

        _context.users.Add(newPatient);
        await _context.SaveChangesAsync();

        var roles        = new List<string> { "Patient" };
        var accessToken  = _tokenService.GenerateAccessToken(newPatient.id, newPatient.email, newPatient.full_name, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        newPatient.refresh_token        = refreshToken;
        newPatient.refresh_token_expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _context.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        return AuthResult.Success(accessToken, refreshToken, expiresAt, MapToDto(newPatient));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var patient = await _context.users
            .FirstOrDefaultAsync(u => u.email == request.Email && u.deleted_at == null);

        if (patient == null)
            return AuthResult.Failure("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, patient.password))
            return AuthResult.Failure("Invalid email or password.");

        if (patient.is_banned)
            return AuthResult.Failure("Your account has been suspended.", patient.ban_reason ?? "Contact support.");

        if (!patient.is_active)
            return AuthResult.Failure("Your account is not active.");

        var roles        = new List<string> { "Patient" };
        var accessToken  = _tokenService.GenerateAccessToken(patient.id, patient.email, patient.full_name, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        patient.refresh_token        = refreshToken;
        patient.refresh_token_expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        patient.updated_at           = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        return AuthResult.Success(accessToken, refreshToken, expiresAt, MapToDto(patient));
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var patient = await _context.users
            .FirstOrDefaultAsync(u => u.refresh_token == refreshToken && u.deleted_at == null);

        if (patient == null || patient.refresh_token_expiry < DateTime.UtcNow)
            return AuthResult.Failure("Invalid or expired refresh token.");

        var roles           = new List<string> { "Patient" };
        var newAccessToken  = _tokenService.GenerateAccessToken(patient.id, patient.email, patient.full_name, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        patient.refresh_token        = newRefreshToken;
        patient.refresh_token_expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        patient.updated_at           = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        return AuthResult.Success(newAccessToken, newRefreshToken, expiresAt, MapToDto(patient));
    }

    public async Task<bool> RevokeTokenAsync(long userId)
    {
        var patient = await _context.users.FindAsync(userId);
        if (patient == null) return false;

        patient.refresh_token        = null;
        patient.refresh_token_expiry = null;
        patient.updated_at           = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // ── Helper ─────────────────────────────────────────────

    private static PatientDto MapToDto(user u) => new()
    {
        Id             = u.id,
        FullName       = u.full_name,
        Email          = u.email,
        MobileNumber   = u.mobile_number,
        ProfilePhotoUrl = u.profile_photo_url,
        Birthdate      = u.birthdate,
        IsActive       = u.is_active,
        CreatedAt      = u.created_at
    };
}
