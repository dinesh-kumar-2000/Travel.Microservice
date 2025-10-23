using MediatR;

namespace IdentityService.Application.Commands.User;

public class DeleteUserCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
}
