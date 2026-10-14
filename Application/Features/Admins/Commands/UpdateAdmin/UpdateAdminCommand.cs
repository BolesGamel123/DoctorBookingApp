using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Admins.DTOs;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Application.Features.Admins.Commands.UpdateAdmin;

public sealed record UpdateAdminCommand(
    long Id,
    string FullName,
    string Email,
    string Role
) : IRequest<ApiResponse<AdminResponse>>;

public sealed class UpdateAdminCommandValidator : AbstractValidator<UpdateAdminCommand>
{
    public UpdateAdminCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Role).Equal("admin");
    }
}

public sealed class UpdateAdminCommandHandler
    : IRequestHandler<UpdateAdminCommand, ApiResponse<AdminResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateAdminCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AdminResponse>> Handle(
        UpdateAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = await _unitOfWork.Repository<Domain.Entities.admin>().GetByIdAsync(request.Id, cancellationToken);

        if (admin is null)
            return ApiResponse<AdminResponse>.Fail("Admin not found.");

        admin.full_name  = request.FullName;
        admin.email      = request.Email;
        admin.role       = request.Role;
        admin.updated_at = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<AdminResponse>.Ok(_mapper.Map<AdminResponse>(admin), "Admin updated.");
    }
}
