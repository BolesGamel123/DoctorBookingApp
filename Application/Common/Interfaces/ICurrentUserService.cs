namespace Application.Common.Interfaces;

/// <summary>
/// Extracts the current authenticated user's identity from the HTTP context.
/// Implemented in Infrastructure/API layer via HttpContextAccessor.
/// </summary>
public interface ICurrentUserService
{
    long? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
