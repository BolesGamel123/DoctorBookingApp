namespace Application.DTOs;

// ── Auth DTOs ──────────────────────────────────────────────

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthResult
{
    public bool Succeeded { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public PatientDto? Patient { get; set; }
    public List<string> Errors { get; set; } = [];

    public static AuthResult Success(string accessToken, string refreshToken, DateTime expiresAt, PatientDto patient)
        => new() { Succeeded = true, AccessToken = accessToken, RefreshToken = refreshToken, ExpiresAt = expiresAt, Patient = patient };

    public static AuthResult Failure(params string[] errors)
        => new() { Succeeded = false, Errors = [.. errors] };
}

// ── Patient DTOs ───────────────────────────────────────────

public class PatientDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
    public DateOnly? Birthdate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdatePatientRequest
{
    public string? FullName { get; set; }
    public string? MobileNumber { get; set; }
    public DateOnly? Birthdate { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? LocationLabel { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

// ── Generic API Response ───────────────────────────────────

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}
