using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Patients.DTOs;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Patients.Commands.UpdatePatient;

public sealed record UpdatePatientCommand(
    long PatientId,
    string? FullName,
    string? MobileNumber,
    DateOnly? Birthdate,
    string? ProfilePhotoUrl,
    string? LocationLabel,
    decimal? Latitude,
    decimal? Longitude
) : IRequest<ApiResponse<PatientResponse>>;

public sealed class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(200).When(x => x.FullName is not null)
            .WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.MobileNumber)
            .MaximumLength(20).When(x => x.MobileNumber is not null)
            .WithMessage("Mobile number must not exceed 20 characters.");
    }
}

public sealed class UpdatePatientCommandHandler
    : IRequestHandler<UpdatePatientCommand, ApiResponse<PatientResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePatientCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PatientResponse>> Handle(
        UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Repository<Domain.Entities.patient>().Query()
            .FirstOrDefaultAsync(u => u.id == request.PatientId && u.deleted_at == null,
                cancellationToken);

        if (patient is null)
            return ApiResponse<PatientResponse>.Fail("Patient not found.");

        if (request.FullName is not null)        patient.full_name        = request.FullName;
        if (request.MobileNumber is not null)    patient.mobile_number    = request.MobileNumber;
        if (request.Birthdate is not null)       patient.birthdate        = request.Birthdate;
        if (request.ProfilePhotoUrl is not null) patient.profile_photo_url = request.ProfilePhotoUrl;
        if (request.LocationLabel is not null)   patient.location_label   = request.LocationLabel;
        if (request.Latitude is not null)        patient.latitude         = request.Latitude;
        if (request.Longitude is not null)       patient.longitude        = request.Longitude;

        patient.updated_at = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<PatientResponse>.Ok(_mapper.Map<PatientResponse>(patient), "Profile updated.");
    }
}
