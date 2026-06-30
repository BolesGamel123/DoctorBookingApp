using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Patients.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Patients.Queries.GetCurrentPatient;

/// <summary>Query to get the currently authenticated patient's profile.</summary>
public sealed record GetCurrentPatientQuery(long PatientId)
    : IRequest<ApiResponse<PatientResponse>>;

public sealed class GetCurrentPatientQueryHandler
    : IRequestHandler<GetCurrentPatientQuery, ApiResponse<PatientResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCurrentPatientQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PatientResponse>> Handle(
        GetCurrentPatientQuery request, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Repository<Domain.Entities.patient>().Query()
            .FirstOrDefaultAsync(u => u.id == request.PatientId && u.deleted_at == null,
                cancellationToken);

        if (patient is null)
            return ApiResponse<PatientResponse>.Fail("Patient not found.");

        return ApiResponse<PatientResponse>.Ok(_mapper.Map<PatientResponse>(patient));
    }
}
