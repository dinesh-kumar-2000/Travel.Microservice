using MediatR;
using IdentityService.Domain.Repositories;
using IdentityService.Contracts.DTOs;

namespace IdentityService.Application.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null || user.TenantId != request.TenantId)
            return null;

        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);

        return new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber, roles);
    }
}

