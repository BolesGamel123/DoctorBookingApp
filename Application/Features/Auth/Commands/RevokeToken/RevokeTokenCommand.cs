using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.RevokeToken;

public sealed record RevokeTokenCommand(long UserId) : IRequest<ApiResponse<string>>;
