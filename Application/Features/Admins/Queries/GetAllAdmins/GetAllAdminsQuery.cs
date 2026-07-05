using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Admins.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admins.Queries.GetAllAdmins;

public sealed record GetAllAdminsQuery(int Page = 1, int PageSize = 20)
    : IRequest<ApiResponse<PagedResult<AdminResponse>>>;

public sealed class GetAllAdminsQueryHandler
    : IRequestHandler<GetAllAdminsQuery, ApiResponse<PagedResult<AdminResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper     _mapper;

    public GetAllAdminsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
    }

    public async Task<ApiResponse<PagedResult<AdminResponse>>> Handle(
        GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        var page     = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _unitOfWork.Repository<Domain.Entities.admin>().Query();

        // Run COUNT and page-data concurrently
        var countTask = query.CountAsync(cancellationToken);
        var itemsTask = query
            .OrderBy(a => a.id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        await Task.WhenAll(countTask, itemsTask);

        var result = new PagedResult<AdminResponse>
        {
            Items      = _mapper.Map<List<AdminResponse>>(itemsTask.Result),
            TotalCount = countTask.Result,
            Page       = page,
            PageSize   = pageSize
        };

        return ApiResponse<PagedResult<AdminResponse>>.Ok(result);
    }
}
