using Application.Common.Models;
using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? AccountType = null
) : IRequest<ApiResponse<AuthResponse>>;
