using Application.Common.Models;
using Application.Features.Doctors.DTOs;
using MediatR;

namespace Application.Features.Doctors.Queries.GetNearbyDoctors;

public sealed record GetNearbyDoctorsQuery(
    decimal Latitude,
    decimal Longitude,
    double RadiusKm = 10,
    int Limit = 20
) : IRequest<ApiResponse<List<NearbyDoctorResponse>>>;