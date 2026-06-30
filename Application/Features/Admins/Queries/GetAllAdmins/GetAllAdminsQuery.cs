using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Admins.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admins.Queries.GetAllAdmins;

public sealed record GetAllAdminsQuery : IRequest<ApiResponse<List<AdminResponse>>>;

public sealed class GetAllAdminsQueryHandler
    : IRequestHandler<GetAllAdminsQuery, ApiResponse<List<AdminResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllAdminsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<AdminResponse>>> Handle(
        GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        var admins = await _unitOfWork.Repository<Domain.Entities.admin>().GetAllAsync(cancellationToken);

        return ApiResponse<List<AdminResponse>>.Ok(_mapper.Map<List<AdminResponse>>(admins));
    }
}
