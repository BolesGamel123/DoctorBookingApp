namespace Application.Features.Auth.DTOs;

/// <summary>
/// Output DTO returned after successful authentication operations.
/// </summary>
public sealed class AuthResponse
{
    public bool Succeeded { get; init; }
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public PatientSummary? Patient { get; init; }
    public List<string> Errors { get; init; } = [];

    public static AuthResponse Success(string accessToken, string refreshToken, DateTime expiresAt, PatientSummary patient)
        => new() { Succeeded = true, AccessToken = accessToken, RefreshToken = refreshToken, ExpiresAt = expiresAt, Patient = patient };

    public static AuthResponse Failure(params string[] errors)
        => new() { Succeeded = false, Errors = [.. errors] };
}

/// <summary>
/// Lightweight patient projection returned inside AuthResponse.
/// </summary>
public sealed class PatientSummary
{
    public long Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MobileNumber { get; init; } = string.Empty;
    public string? ProfilePhotoUrl { get; init; }
    public DateOnly? Birthdate { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
