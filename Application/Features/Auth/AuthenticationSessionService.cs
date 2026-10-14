using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Features.Auth;

public sealed class AuthenticationSessionService(
    IApplicationDbContext context, ITokenService tokens, IOptions<JwtSettingsOption> options)
    : IAuthenticationSessionService
{
    private readonly JwtSettingsOption _settings = options.Value;

    public async Task<AuthResponse> IssueAsync(AuthAccount account, CancellationToken cancellationToken)
    {
        if (!account.CanAuthenticate || !AccountRoles.IsValid(account.Role))
            throw new UnauthorizedAccessException();

        var session = await context.AuthenticationSessions.SingleOrDefaultAsync(
            x => x.Role == account.Role && x.UserId == account.Summary.Id, cancellationToken);
        if (session is null)
        {
            session = new AuthenticationSession { Role = account.Role, UserId = account.Summary.Id };
            context.AuthenticationSessions.Add(session);
        }

        session.SessionId = Guid.NewGuid();
        session.RevokedAt = null;
        var response = Rotate(session, account);
        await context.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || refreshToken.Length > 128)
            return null;

        var hash = tokens.HashRefreshToken(refreshToken);
        var session = await context.AuthenticationSessions.SingleOrDefaultAsync(
            x => x.RefreshTokenHash == hash, cancellationToken);
        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= DateTime.UtcNow)
            return null;

        var account = await AuthAccount.FindAsync(context, session.Role, session.UserId, cancellationToken);
        if (account is null || !account.CanAuthenticate)
            return null;

        var response = Rotate(session, account);
        try
        {
            // RefreshTokenHash is an EF concurrency token: only one request can consume the old hash.
            await context.SaveChangesAsync(cancellationToken);
            return response;
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }
    }

    public async Task<bool> RevokeAsync(string role, long userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await context.AuthenticationSessions.SingleOrDefaultAsync(
            x => x.Role == role && x.UserId == userId && x.SessionId == sessionId, cancellationToken);
        if (session is null || session.RevokedAt is not null)
            return false;

        session.RevokedAt = DateTime.UtcNow;
        session.RefreshTokenHash = string.Empty;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Report failure instead of claiming logout succeeded after a concurrent session change.
            return false;
        }
    }

    public async Task<bool> IsValidAsync(string role, long userId, Guid sessionId, CancellationToken cancellationToken)
    {
        if (!AccountRoles.IsValid(role)) return false;
        var exists = await context.AuthenticationSessions.AsNoTracking().AnyAsync(
            x => x.Role == role && x.UserId == userId && x.SessionId == sessionId
                && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow, cancellationToken);
        if (!exists) return false;
        var account = await AuthAccount.FindAsync(context, role, userId, cancellationToken);
        return account?.CanAuthenticate == true;
    }

    private AuthResponse Rotate(AuthenticationSession session, AuthAccount account)
    {
        var refreshToken = tokens.GenerateRefreshToken();
        session.RefreshTokenHash = tokens.HashRefreshToken(refreshToken);
        session.ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
        var accessToken = tokens.GenerateAccessToken(account.Summary.Id, account.Summary.Email,
            account.Summary.FullName, [account.Role], session.SessionId);
        return AuthResponse.Success(accessToken, refreshToken,
            DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes), account.Summary, account.Role);
    }
}
