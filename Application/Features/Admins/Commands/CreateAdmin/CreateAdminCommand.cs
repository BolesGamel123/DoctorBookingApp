using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Admins.DTOs;
using AutoMapper;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.Admins.Commands.CreateAdmin;

public sealed record CreateAdminCommand(
    string FullName,
    string Email,
    string Password,
    string Role
) : IRequest<ApiResponse<AdminResponse>>;

public sealed class CreateAdminCommandValidator : AbstractValidator<CreateAdminCommand>
{
    public CreateAdminCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Role).NotEmpty().MaximumLength(20);
    }
}

public sealed class CreateAdminCommandHandler
    : IRequestHandler<CreateAdminCommand, ApiResponse<AdminResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public CreateAdminCommandHandler(
        IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<ApiResponse<AdminResponse>> Handle(
        CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var newAdmin = new admin
        {
            full_name  = request.FullName,
            email      = request.Email,
            password   = _passwordHasher.Hash(request.Password),
            role       = request.Role,
            is_active  = true,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _unitOfWork.Repository<admin>().AddAsync(newAdmin);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<AdminResponse>.Ok(
            _mapper.Map<AdminResponse>(newAdmin), "Admin created.");
    }
}
