using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtSettingsOption _jwtSettings;

    public LoginCommandHandler(
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
    LoginCommand request,
    CancellationToken cancellationToken)
    {
        var patient = await _context.patients
            .FirstOrDefaultAsync(u =>
                u.email == request.Email &&
                u.deleted_at == null,
                cancellationToken);

        if (patient is not null)
        {
            if (!_passwordHasher.Verify(request.Password, patient.password))
                return ApiResponse<AuthResponse>.Fail("Login failed.", ["Invalid email or password."]);

            if (patient.is_banned)
                return ApiResponse<AuthResponse>.Fail("Login failed.",
                    ["Your account has been suspended.", patient.ban_reason ?? "Contact support."]);

            if (!patient.is_active)
                return ApiResponse<AuthResponse>.Fail("Login failed.", ["Your account is not active."]);

            var roles = new List<string> { "Patient" };

            var accessToken = _tokenService.GenerateAccessToken(
                patient.id,
                patient.email,
                patient.full_name,
                roles);

            var refreshToken = _tokenService.GenerateRefreshToken();

            patient.refresh_token = refreshToken;
            patient.refresh_token_expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            patient.updated_at = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            var summary = new PatientSummary
            {
                Id = patient.id,
                FullName = patient.full_name,
                Email = patient.email,
                MobileNumber = patient.mobile_number,
                ProfilePhotoUrl = patient.profile_photo_url,
                Birthdate = patient.birthdate,
                IsActive = patient.is_active,
                CreatedAt = patient.created_at
            };

            return ApiResponse<AuthResponse>.Ok(
                AuthResponse.Success(accessToken, refreshToken, expiresAt, summary),
                "Login successful.");
        }

        var doctor = await _context.doctors
            .FirstOrDefaultAsync(u =>
                u.email == request.Email &&
                u.deleted_at == null,
                cancellationToken);

        if (doctor is not null)
        {
            if (!_passwordHasher.Verify(request.Password, doctor.password))
                return ApiResponse<AuthResponse>.Fail("Login failed.", ["Invalid email or password."]);

            if (doctor.is_banned)
                return ApiResponse<AuthResponse>.Fail("Login failed.", ["Your account has been suspended."]);

            if (!doctor.is_active)
                return ApiResponse<AuthResponse>.Fail("Login failed.", ["Your account is not active."]);

            var roles = new List<string> { "Doctor" };

            var accessToken = _tokenService.GenerateAccessToken(
                doctor.id,
                doctor.email,
                doctor.full_name,
                roles);

            var refreshToken = _tokenService.GenerateRefreshToken();

            doctor.updated_at = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            var summary = new PatientSummary
            {
                Id = doctor.id,
                FullName = doctor.full_name,
                Email = doctor.email,
                MobileNumber = doctor.mobile_number,
                ProfilePhotoUrl = doctor.profile_photo_url,
                Birthdate = null,
                IsActive = doctor.is_active,
                CreatedAt = doctor.created_at
            };

            return ApiResponse<AuthResponse>.Ok(
                AuthResponse.Success(accessToken, refreshToken, expiresAt, summary),
                "Login successful.");
        }

        return ApiResponse<AuthResponse>.Fail("Login failed.", ["Invalid email or password."]);
    }
}
