using MediatR;
using Microsoft.Extensions.Logging;
using IdentityService.Application.Interfaces;
using IdentityService.Application.DTOs;
using SharedKernel.Exceptions;

namespace IdentityService.Application.Commands;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UpdateProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating profile for user {UserId}", request.UserId);

        // Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            throw new NotFoundException("User not found");
        }

        // Update profile
        user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Get user roles for response
        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);

        _logger.LogInformation("Successfully updated profile for user {UserId}", request.UserId);

        return new UpdateProfileResponse(
            true,
            "Profile updated successfully",
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber, roles)
        );
    }
}

