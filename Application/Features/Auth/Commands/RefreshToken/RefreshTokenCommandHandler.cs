using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(IAuthenticationSessionService sessions)
    : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
    public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var response = await sessions.RefreshAsync(request.RefreshToken, cancellationToken);
        return response is null
            ? ApiResponse<AuthResponse>.Fail("Token refresh failed.", ["Invalid or expired refresh token."])
            : ApiResponse<AuthResponse>.Ok(response, "Token refreshed.");
    }
}
