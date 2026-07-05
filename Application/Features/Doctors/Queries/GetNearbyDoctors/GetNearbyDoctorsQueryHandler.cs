using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Doctors.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Doctors.Queries.GetNearbyDoctors;

public sealed class GetNearbyDoctorsQueryHandler
    : IRequestHandler<GetNearbyDoctorsQuery, ApiResponse<List<NearbyDoctorResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetNearbyDoctorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<NearbyDoctorResponse>>> Handle(
        GetNearbyDoctorsQuery request,
        CancellationToken cancellationToken)
    {
        // Validate latitude and longitude
        if (request.Latitude < -90 || request.Latitude > 90)
            return ApiResponse<List<NearbyDoctorResponse>>.Fail(
                "Invalid latitude.",
                ["Latitude must be between -90 and 90."]);

        // Validate longitude
        if (request.Longitude < -180 || request.Longitude > 180)
            return ApiResponse<List<NearbyDoctorResponse>>.Fail(
                "Invalid longitude.",
                ["Longitude must be between -180 and 180."]);

        // Validate radius
        var radiusKm = request.RadiusKm <= 0 ? 10 : request.RadiusKm;

        // Validate limit   
        var limit = request.Limit <= 0 ? 20 : request.Limit;
        // Ensure the limit does not exceed 100
        limit = Math.Min(limit, 100);

        var patientLat = (double)request.Latitude;
        var patientLng = (double)request.Longitude;

        // ── Bounding-box pre-filter ────────────────────────────────────────
        // 1° of latitude ≈ 111 km. Longitude degree length shrinks with cos(lat).
        // This lets the DB use an index on clinic_latitude / clinic_longitude and
        // avoids loading the entire doctors table into memory.
        var latDelta  = (decimal)(radiusKm / 111.0);
        var lngDelta  = (decimal)(radiusKm / (111.0 * Math.Cos(patientLat * Math.PI / 180.0)));

        var latMin = request.Latitude  - latDelta;
        var latMax = request.Latitude  + latDelta;
        var lngMin = request.Longitude - lngDelta;
        var lngMax = request.Longitude + lngDelta;

        // Note: .Include() is intentionally omitted — EF Core ignores eager-loads
        // when the query is immediately projected via .Select(), so they only waste
        // a round-trip. Navigation properties are accessed inline in the projection.
        var doctors = await _context.doctors
            .AsNoTracking()
            .Where(d =>
                d.deleted_at == null &&
                d.is_active &&
                d.is_verified &&
                !d.is_banned &&
                d.clinic_latitude  != null &&
                d.clinic_longitude != null &&
                d.clinic_latitude  >= latMin && d.clinic_latitude  <= latMax &&
                d.clinic_longitude >= lngMin && d.clinic_longitude <= lngMax)
            .Select(d => new
            {
                d.id,
                d.full_name,
                d.profile_photo_url,
                d.specialty_id,
                SpecialtyName  = d.specialty != null ? d.specialty.name : null,
                d.clinic_id,
                ClinicName     = d.clinic != null ? d.clinic.name    : null,
                ClinicAddress  = d.clinic != null ? d.clinic.address : null,
                d.clinic_latitude,
                d.clinic_longitude,
                d.rating_avg,
                d.rating_count,
                d.session_price,
                d.currency
            })
            .ToListAsync(cancellationToken);

        // ── Exact Haversine filter in-memory (on the already-small candidate set) ──
        var nearbyDoctors = doctors
            .Select(d =>
            {
                var distanceKm = CalculateDistanceKm(
                    patientLat,
                    patientLng,
                    (double)d.clinic_latitude!.Value,
                    (double)d.clinic_longitude!.Value);

                return new NearbyDoctorResponse
                {
                    Id              = d.id,
                    FullName        = d.full_name,
                    ProfilePhotoUrl = d.profile_photo_url,
                    SpecialtyId     = d.specialty_id,
                    SpecialtyName   = d.SpecialtyName,
                    ClinicId        = d.clinic_id,
                    ClinicName      = d.ClinicName,
                    ClinicAddress   = d.ClinicAddress,
                    ClinicLatitude  = d.clinic_latitude,
                    ClinicLongitude = d.clinic_longitude,
                    RatingAvg       = d.rating_avg,
                    RatingCount     = d.rating_count,
                    SessionPrice    = d.session_price,
                    Currency        = d.currency,
                    DistanceKm      = Math.Round(distanceKm, 2)
                };
            })
            .Where(d => d.DistanceKm <= radiusKm)   // exact radius (bounding-box may over-include corners)
            .OrderBy(d => d.DistanceKm)
            .ThenByDescending(d => d.RatingAvg)
            .Take(limit)
            .ToList();

        // Return the list of nearby doctors in the API response
        return ApiResponse<List<NearbyDoctorResponse>>.Ok(
            nearbyDoctors,
            "Nearby doctors retrieved successfully.");
    }

    private static double CalculateDistanceKm(
        double lat1,
        double lon1,
        double lat2,
        double lon2)
    {
        const double earthRadiusKm = 6371;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) *
            Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) *
            Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}