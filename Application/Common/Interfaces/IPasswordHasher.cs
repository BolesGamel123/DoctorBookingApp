namespace Application.Common.Interfaces;

/// <summary>
/// Abstracts password hashing so BCrypt stays in Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hashedPassword);
}
