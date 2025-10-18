using MediatR;
using TenantService.Contracts.DTOs;

namespace TenantService.Application.Queries;

public record GetActiveTenantsQuery : IRequest<IEnumerable<TenantDto>>;

