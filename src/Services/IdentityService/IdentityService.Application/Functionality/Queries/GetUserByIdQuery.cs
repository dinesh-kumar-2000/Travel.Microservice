using MediatR;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Queries;

public record GetUserByIdQuery(string UserId, string TenantId) : IRequest<UserDto?>;

