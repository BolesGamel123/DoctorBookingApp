using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly JwtSettingsOption _jwtSettings;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IOptions<JwtSettingsOption> jwtSettings)
    {
        _context = context;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var patient = await _context.patients
            .FirstOrDefaultAsync(u => u.refresh_token == request.RefreshToken && u.deleted_at == null,
                cancellationToken);

        if (patient is null || patient.refresh_token_expiry < DateTime.UtcNow)
            return ApiResponse<AuthResponse>.Fail("Token refresh failed.",
                ["Invalid or expired refresh token."]);

        var roles           = new List<string> { "Patient" };
        var newAccessToken  = _tokenService.GenerateAccessToken(patient.id, patient.email, patient.full_name, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        patient.refresh_token        = newRefreshToken;
        patient.refresh_token_expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        patient.updated_at           = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var summary = new PatientSummary
        {
            Id = patient.id, FullName = patient.full_name, Email = patient.email,
            MobileNumber = patient.mobile_number, ProfilePhotoUrl = patient.profile_photo_url,
            Birthdate = patient.birthdate, IsActive = patient.is_active, CreatedAt = patient.created_at
        };

        return ApiResponse<AuthResponse>.Ok(
            AuthResponse.Success(newAccessToken, newRefreshToken, expiresAt, summary),
            "Token refreshed.");
    }
}
