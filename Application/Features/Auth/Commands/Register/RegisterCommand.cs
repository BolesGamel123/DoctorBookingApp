using Application.Common.Models;
using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new patient account.
/// </summary>
public sealed record RegisterCommand(
    string FullName,
    string Email,
    string MobileNumber,
    string Password,
    string ConfirmPassword
) : IRequest<ApiResponse<AuthResponse>>;
