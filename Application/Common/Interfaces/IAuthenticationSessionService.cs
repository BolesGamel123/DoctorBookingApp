using Application.Features.Auth;
using Application.Features.Auth.DTOs;

namespace Application.Common.Interfaces;

public interface IAuthenticationSessionService
{
    Task<AuthResponse> IssueAsync(AuthAccount account, CancellationToken cancellationToken);
    Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task<bool> RevokeAsync(string role, long userId, Guid sessionId, CancellationToken cancellationToken);
    Task<bool> IsValidAsync(string role, long userId, Guid sessionId, CancellationToken cancellationToken);
}
