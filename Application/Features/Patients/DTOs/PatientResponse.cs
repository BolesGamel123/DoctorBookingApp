namespace Application.Features.Patients.DTOs;

/// <summary>
/// Output DTO for patient data. Never expose domain entities to the API.
/// </summary>
public sealed class PatientResponse
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
