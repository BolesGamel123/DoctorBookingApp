namespace Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(long userId, string email, string fullName, IList<string> roles);
    string GenerateRefreshToken();
}
