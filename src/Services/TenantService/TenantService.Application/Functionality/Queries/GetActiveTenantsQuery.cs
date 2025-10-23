using MediatR;
using TenantService.Application.DTOs;

namespace TenantService.Application.Queries;

public record GetActiveTenantsQuery : IRequest<IEnumerable<TenantDto>>;

