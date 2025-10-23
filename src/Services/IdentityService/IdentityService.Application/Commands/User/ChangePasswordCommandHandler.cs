using MediatR;
using Microsoft.Extensions.Logging;
using IdentityService.Domain.Repositories;
using SharedKernel.Exceptions;
using BCrypt.Net;

namespace IdentityService.Application.Commands.User;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Changing password for user {UserId}", request.UserId);

        var user = await _userRepository.GetByIdAsync(request.UserId.ToString(), cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Invalid current password for user {UserId}", request.UserId);
            throw new UnauthorizedException("Current password is incorrect");
        }

        // Update password
        user.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Successfully changed password for user {UserId}", request.UserId);

        return Unit.Value;
    }
}
