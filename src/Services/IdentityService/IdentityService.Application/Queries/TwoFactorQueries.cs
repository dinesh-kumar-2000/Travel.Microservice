using IdentityService.Contracts.DTOs;
using MediatR;

namespace IdentityService.Application.Queries;

public record GetTwoFactorStatusQuery(Guid UserId) 
    : IRequest<TwoFactorStatusDto>;

