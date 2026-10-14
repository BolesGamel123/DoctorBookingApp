using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth;

public sealed record AuthAccount(string Role, string PasswordHash, bool CanAuthenticate, PatientSummary Summary)
{
    public static AuthAccount From(patient account) => new(AccountRoles.Patient, account.password,
        account.deleted_at is null && account.is_active && !account.is_banned, new PatientSummary
        {
            Id = account.id, Email = account.email, FullName = account.full_name,
            MobileNumber = account.mobile_number, ProfilePhotoUrl = account.profile_photo_url,
            Birthdate = account.birthdate, IsActive = account.is_active, CreatedAt = account.created_at
        });

    public static AuthAccount From(doctor account) => new(AccountRoles.Doctor, account.password,
        account.deleted_at is null && account.is_active && !account.is_banned && !account.must_change_password,
        new PatientSummary
        {
            Id = account.id, Email = account.email, FullName = account.full_name,
            MobileNumber = account.mobile_number, ProfilePhotoUrl = account.profile_photo_url,
            IsActive = account.is_active, CreatedAt = account.created_at
        });

    // Never bypass an existing MFA requirement. A separate second-factor flow is needed for these accounts.
    public static AuthAccount From(admin account) => new(AccountRoles.Admin, account.password,
        account.is_active && !account.two_factor_enabled
            && string.Equals(account.role, "admin", StringComparison.OrdinalIgnoreCase), new PatientSummary
        {
            Id = account.id, Email = account.email, FullName = account.full_name,
            IsActive = account.is_active, CreatedAt = account.created_at
        });

    public static async Task<AuthAccount?> FindAsync(
        IApplicationDbContext context, string role, long id, CancellationToken cancellationToken)
    {
        switch (role)
        {
            case AccountRoles.Patient:
                var patient = await context.patients.AsNoTracking().SingleOrDefaultAsync(x => x.id == id, cancellationToken);
                return patient is null ? null : From(patient);
            case AccountRoles.Doctor:
                var doctor = await context.doctors.AsNoTracking().SingleOrDefaultAsync(x => x.id == id, cancellationToken);
                return doctor is null ? null : From(doctor);
            case AccountRoles.Admin:
                var admin = await context.admins.AsNoTracking().SingleOrDefaultAsync(x => x.id == id, cancellationToken);
                return admin is null ? null : From(admin);
            default:
                return null;
        }
    }
}
