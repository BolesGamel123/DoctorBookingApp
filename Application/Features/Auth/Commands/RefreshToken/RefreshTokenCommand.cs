using Application.Common.Models;
using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken
) : IRequest<ApiResponse<AuthResponse>>;
