using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IApplicationDbContext context, IPasswordHasher passwordHasher, IAuthenticationSessionService sessions)
    : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
{
    public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var candidates = new List<AuthAccount>();
        if (request.AccountType is null or AccountRoles.Patient)
        {
            var patient = await context.patients.SingleOrDefaultAsync(
                x => x.email == request.Email && x.deleted_at == null, cancellationToken);
            if (patient is not null) candidates.Add(AuthAccount.From(patient));
        }
        if (request.AccountType is null or AccountRoles.Doctor)
        {
            var doctor = await context.doctors.SingleOrDefaultAsync(
                x => x.email == request.Email && x.deleted_at == null, cancellationToken);
            if (doctor is not null) candidates.Add(AuthAccount.From(doctor));
        }
        // Public login never falls back to privileged accounts.
        if (request.AccountType == AccountRoles.Admin)
        {
            var admin = await context.admins.SingleOrDefaultAsync(x => x.email == request.Email, cancellationToken);
            if (admin is not null) candidates.Add(AuthAccount.From(admin));
        }
        if (candidates.Count != 1 || !candidates[0].CanAuthenticate
            || !passwordHasher.Verify(request.Password, candidates[0].PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Login failed.", ["Invalid credentials or account unavailable."]);

        try
        {
            var response = await sessions.IssueAsync(candidates[0], cancellationToken);
            return ApiResponse<AuthResponse>.Ok(response, "Login successful.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return ApiResponse<AuthResponse>.Fail("Login failed.", ["Session changed; please retry login."]);
        }
    }
}
