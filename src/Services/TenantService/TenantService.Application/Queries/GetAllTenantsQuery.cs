using MediatR;
using TenantService.Contracts.DTOs;

namespace TenantService.Application.Queries;

public record GetAllTenantsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Status = null
) : IRequest<PagedTenantsResponse>;

