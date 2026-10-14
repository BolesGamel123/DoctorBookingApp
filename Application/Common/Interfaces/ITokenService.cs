namespace Application.Common.Interfaces;

/// <summary>
/// Generates JWT access tokens and opaque refresh tokens.
/// Implemented in Infrastructure layer.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(long userId, string email, string fullName, IList<string> roles, Guid sessionId);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
