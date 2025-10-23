using MediatR;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Queries;

public record GetProfileQuery(
    string UserId
) : IRequest<GetProfileResponse>;

