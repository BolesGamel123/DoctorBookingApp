using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.RevokeToken;

public sealed class RevokeTokenCommandHandler(IAuthenticationSessionService sessions)
    : IRequestHandler<RevokeTokenCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        => await sessions.RevokeAsync(request.Role, request.UserId, request.SessionId, cancellationToken)
            ? ApiResponse<string>.Ok("Session revoked.", "Logged out successfully.")
            : ApiResponse<string>.Fail("Session changed or was already revoked; retry logout if still authenticated.");
}
