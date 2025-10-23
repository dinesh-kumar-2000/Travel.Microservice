using MediatR;
using IdentityService.Application.DTOs.Responses.User;
using SharedKernel.Models;

namespace IdentityService.Application.Queries.User;

public class GetUserQuery : IRequest<UserResponse?>
{
    public Guid UserId { get; set; }
}
