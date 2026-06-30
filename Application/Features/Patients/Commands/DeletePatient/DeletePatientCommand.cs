using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Patients.Commands.DeletePatient;

public sealed record DeletePatientCommand(long Id) : IRequest<ApiResponse<string>>;

public sealed class DeletePatientCommandHandler
    : IRequestHandler<DeletePatientCommand, ApiResponse<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePatientCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<string>> Handle(
        DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Repository<Domain.Entities.patient>().GetByIdAsync(request.Id, cancellationToken);

        if (patient is null || patient.deleted_at is not null)
            return ApiResponse<string>.Fail("Patient not found.");

        // Soft delete
        patient.deleted_at           = DateTime.UtcNow;
        patient.is_active            = false;
        patient.refresh_token        = null;
        patient.refresh_token_expiry = null;
        patient.updated_at           = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.Ok("Patient deleted.", "Patient soft-deleted successfully.");
    }
}
