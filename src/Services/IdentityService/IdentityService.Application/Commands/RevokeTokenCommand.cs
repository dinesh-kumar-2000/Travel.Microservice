using MediatR;
using IdentityService.Contracts.DTOs;

namespace IdentityService.Application.Commands;

public record RevokeTokenCommand(
    string UserId
) : IRequest<RevokeTokenResponse>;

