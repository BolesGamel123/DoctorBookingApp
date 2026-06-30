using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, ApiResponse<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtSettingsOption _jwtSettings;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IOptions<JwtSettingsOption> jwtSettings)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(
        RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate email
        if (await _context.patients.AnyAsync(
                u => u.email == request.Email && u.deleted_at == null, cancellationToken))
            return ApiResponse<AuthResponse>.Fail("Registration failed.",
                ["Email is already registered."]);

        // Check for duplicate mobile
        if (await _context.patients.AnyAsync(
                u => u.mobile_number == request.MobileNumber && u.deleted_at == null, cancellationToken))
            return ApiResponse<AuthResponse>.Fail("Registration failed.",
                ["Mobile number is already registered."]);

        var newPatient = new patient
        {
            full_name     = request.FullName,
            email         = request.Email,
            mobile_number = request.MobileNumber,
            password      = _passwordHasher.Hash(request.Password),
            is_active     = true,
            is_banned     = false,
            remember_me   = false,
            created_at    = DateTime.UtcNow,
            updated_at    = DateTime.UtcNow
        };

        _context.patients.Add(newPatient);
        await _context.SaveChangesAsync(cancellationToken);

        var roles        = new List<string> { "Patient" };
        var accessToken  = _tokenService.GenerateAccessToken(newPatient.id, newPatient.email, newPatient.full_name, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        newPatient.refresh_token        = refreshToken;
        newPatient.refresh_token_expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        await _context.SaveChangesAsync(cancellationToken);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var result = AuthResponse.Success(accessToken, refreshToken, expiresAt, MapToSummary(newPatient));

        return ApiResponse<AuthResponse>.Ok(result, "Registration successful.");
    }

    private static PatientSummary MapToSummary(patient u) => new()
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

/// <summary>
/// Lightweight options class for JWT settings.
/// Bound from appsettings.json "JwtSettings" section.
/// </summary>
public sealed class JwtSettingsOption
{
    public const string SectionName = "JwtSettings";
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "DoctorBookingApp";
    public string Audience { get; set; } = "DoctorBookingApp";
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
