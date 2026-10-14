using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DoctorBooking.Hubs;

// JWT middleware runs at connection setup; recheck sessions on each subsequent hub invocation.
public sealed class AuthenticationHubFilter(IAuthenticationSessionService sessions) : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var user = invocationContext.Context.User;
        if (user is null
            || !long.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id)
            || !Guid.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Sid), out var sessionId)
            || !await sessions.IsValidAsync(user.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
                id, sessionId, invocationContext.Context.ConnectionAborted))
        {
            invocationContext.Context.Abort();
            throw new HubException("Session is unavailable.");
        }
        return await next(invocationContext);
    }
}
