using MediatR;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Commands;

public record RefreshTokenCommand(
    string RefreshToken,
    string UserId
) : IRequest<RefreshTokenResponse>;

