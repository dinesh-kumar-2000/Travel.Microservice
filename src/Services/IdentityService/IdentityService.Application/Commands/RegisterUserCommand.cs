using MediatR;
using IdentityService.Contracts.DTOs;

namespace IdentityService.Application.Commands;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string TenantId,
    string? Role = null // Optional: SuperAdmin, TenantAdmin, Agent, Customer (default: Customer)
) : IRequest<RegisterUserResponse>;

