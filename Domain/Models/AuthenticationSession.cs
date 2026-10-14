namespace Domain.Entities;

// One current session per account. Role is part of the key because account tables share numeric IDs.
public sealed class AuthenticationSession
{
    public string Role { get; set; } = string.Empty;
    public long UserId { get; set; }
    public Guid SessionId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
