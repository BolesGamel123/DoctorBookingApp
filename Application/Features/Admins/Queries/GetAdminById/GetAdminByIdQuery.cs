using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Admins.DTOs;
using AutoMapper;
using MediatR;

namespace Application.Features.Admins.Queries.GetAdminById;

public sealed record GetAdminByIdQuery(long Id) : IRequest<ApiResponse<AdminResponse>>;

public sealed class GetAdminByIdQueryHandler
    : IRequestHandler<GetAdminByIdQuery, ApiResponse<AdminResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAdminByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AdminResponse>> Handle(
        GetAdminByIdQuery request, CancellationToken cancellationToken)
    {
        var admin = await _unitOfWork.Repository<Domain.Entities.admin>().GetByIdAsync(request.Id, cancellationToken);

        if (admin is null)
            return ApiResponse<AdminResponse>.Fail("Admin not found.");

        return ApiResponse<AdminResponse>.Ok(_mapper.Map<AdminResponse>(admin));
    }
}
