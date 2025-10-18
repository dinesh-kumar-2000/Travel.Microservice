using MediatR;
using IdentityService.Contracts.DTOs;

namespace IdentityService.Application.Commands;

public record LoginCommand(
    string Email,
    string Password,
    string TenantId
) : IRequest<LoginResponse>;

