using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Admins.Commands.DeleteAdmin;

public sealed record DeleteAdminCommand(long Id) : IRequest<ApiResponse<string>>;

public sealed class DeleteAdminCommandHandler
    : IRequestHandler<DeleteAdminCommand, ApiResponse<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAdminCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<string>> Handle(
        DeleteAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = await _unitOfWork.Repository<Domain.Entities.admin>().GetByIdAsync(request.Id, cancellationToken);

        if (admin is null)
            return ApiResponse<string>.Fail("Admin not found.");

        _unitOfWork.Repository<Domain.Entities.admin>().Remove(admin);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.Ok("Admin deleted.", "Admin removed successfully.");
    }
}
