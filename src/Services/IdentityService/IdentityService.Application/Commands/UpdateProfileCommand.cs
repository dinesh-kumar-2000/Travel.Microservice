using MediatR;
using IdentityService.Contracts.DTOs;

namespace IdentityService.Application.Commands;

public record UpdateProfileCommand(
    string UserId,
    string FirstName,
    string LastName,
    string PhoneNumber
) : IRequest<UpdateProfileResponse>;

