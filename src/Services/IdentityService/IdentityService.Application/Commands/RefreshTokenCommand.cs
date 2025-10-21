using MediatR;
using IdentityService.Contracts.DTOs;

namespace IdentityService.Application.Commands;

public record RefreshTokenCommand(
    string RefreshToken,
    string UserId
) : IRequest<RefreshTokenResponse>;

