using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.RevokeToken;

public sealed class RevokeTokenCommandHandler
    : IRequestHandler<RevokeTokenCommand, ApiResponse<string>>
{
    private readonly IApplicationDbContext _context;

    public RevokeTokenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<string>> Handle(
        RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var patient = await _context.patients.FindAsync([request.UserId], cancellationToken);

        if (patient is null)
            return ApiResponse<string>.Fail("Patient not found.");

        patient.refresh_token        = null;
        patient.refresh_token_expiry = null;
        patient.updated_at           = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.Ok("Refresh token revoked.", "Logged out successfully.");
    }
}
