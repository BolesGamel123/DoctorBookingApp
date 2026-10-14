using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.RevokeToken;

public sealed record RevokeTokenCommand(long UserId, string Role, Guid SessionId) : IRequest<ApiResponse<string>>;
