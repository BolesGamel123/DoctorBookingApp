using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, ApiResponse<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthenticationSessionService _sessions;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IAuthenticationSessionService sessions,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _sessions = sessions;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(
        RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate email
        if (await _context.patients.AnyAsync(
                u => u.email == request.Email, cancellationToken))
            return ApiResponse<AuthResponse>.Fail("Registration failed.",
                ["Email is already registered."]);

        // Check for duplicate mobile
        if (await _context.patients.AnyAsync(
                u => u.mobile_number == request.MobileNumber, cancellationToken))
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

        var result = await _sessions.IssueAsync(AuthAccount.From(newPatient), cancellationToken);

        return ApiResponse<AuthResponse>.Ok(result, "Registration successful.");
    }

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
