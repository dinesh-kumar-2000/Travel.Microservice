using MediatR;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Commands;

public record RevokeTokenCommand(
    string UserId
) : IRequest<RevokeTokenResponse>;

