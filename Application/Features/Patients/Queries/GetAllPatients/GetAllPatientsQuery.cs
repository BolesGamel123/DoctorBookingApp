using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Patients.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Patients.Queries.GetAllPatients;

public sealed record GetAllPatientsQuery(int Page = 1, int PageSize = 20)
    : IRequest<ApiResponse<PagedResult<PatientResponse>>>;

public sealed class GetAllPatientsQueryHandler
    : IRequestHandler<GetAllPatientsQuery, ApiResponse<PagedResult<PatientResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllPatientsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<PatientResponse>>> Handle(
        GetAllPatientsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Domain.Entities.patient>().Query().Where(u => u.deleted_at == null);

        var totalCount = await query.CountAsync(cancellationToken);

        var patients = await query
            .OrderByDescending(u => u.created_at)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<PatientResponse>
        {
            Items      = _mapper.Map<List<PatientResponse>>(patients),
            TotalCount = totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize
        };

        return ApiResponse<PagedResult<PatientResponse>>.Ok(result);
    }
}
