using MediatR;
using TenantService.Contracts.DTOs;

namespace TenantService.Application.Queries;

public record GetTenantStatsQuery() : IRequest<TenantStatsResponse>;

