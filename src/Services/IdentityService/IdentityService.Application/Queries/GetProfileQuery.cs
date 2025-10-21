using MediatR;
using IdentityService.Contracts.DTOs;

namespace IdentityService.Application.Queries;

public record GetProfileQuery(
    string UserId
) : IRequest<GetProfileResponse>;

