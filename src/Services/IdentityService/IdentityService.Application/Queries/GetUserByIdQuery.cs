using MediatR;
using IdentityService.Contracts.DTOs;

namespace IdentityService.Application.Queries;

public record GetUserByIdQuery(string UserId, string TenantId) : IRequest<UserDto?>;

