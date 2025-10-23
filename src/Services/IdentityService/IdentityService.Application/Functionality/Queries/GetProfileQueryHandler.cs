using MediatR;
using Microsoft.Extensions.Logging;
using IdentityService.Application.Interfaces;
using IdentityService.Application.DTOs;
using SharedKernel.Exceptions;

namespace IdentityService.Application.Queries;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, GetProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetProfileQueryHandler> _logger;

    public GetProfileQueryHandler(
        IUserRepository userRepository,
        ILogger<GetProfileQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<GetProfileResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting profile for user {UserId}", request.UserId);

        // Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            throw new NotFoundException("User not found");
        }

        // Get user roles
        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);

        _logger.LogInformation("Successfully retrieved profile for user {UserId}", request.UserId);

        return new GetProfileResponse(
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber, roles)
        );
    }
}

