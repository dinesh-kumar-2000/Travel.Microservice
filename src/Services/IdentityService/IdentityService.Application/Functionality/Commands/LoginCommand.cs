using MediatR;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Commands;

public record LoginCommand(
    string Email,
    string Password,
    string? Domain,
    string? TenantId
) : IRequest<LoginResponse>;

