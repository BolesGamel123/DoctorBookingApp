namespace Application.Features.Doctors.DTOs;

public sealed class NearbyDoctorResponse
{
    public long Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string? ProfilePhotoUrl { get; init; }

    public long SpecialtyId { get; init; }

    public string? SpecialtyName { get; init; }

    public long? ClinicId { get; init; }

    public string? ClinicName { get; init; }

    public string? ClinicAddress { get; init; }

    public decimal? ClinicLatitude { get; init; }

    public decimal? ClinicLongitude { get; init; }

    public decimal RatingAvg { get; init; }

    public int RatingCount { get; init; }

    public decimal SessionPrice { get; init; }

    public string Currency { get; init; } = string.Empty;

    public double DistanceKm { get; init; }
}