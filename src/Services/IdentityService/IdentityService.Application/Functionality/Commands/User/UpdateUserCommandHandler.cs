using MediatR;
using Microsoft.Extensions.Logging;
using IdentityService.Application.Interfaces;
using IdentityService.Application.DTOs.Responses.User;
using SharedKernel.Exceptions;

namespace IdentityService.Application.Commands.User;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        IUserRepository userRepository,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user {UserId}", request.UserId);

        var user = await _userRepository.GetByIdAsync(request.UserId.ToString(), cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Update user profile
        user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);

        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Successfully updated user {UserId}", request.UserId);

        return new UserResponse
        {
            Id = Guid.Parse(user.Id),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
