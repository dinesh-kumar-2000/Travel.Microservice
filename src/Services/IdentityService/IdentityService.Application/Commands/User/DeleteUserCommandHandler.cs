using MediatR;
using Microsoft.Extensions.Logging;
using IdentityService.Domain.Repositories;
using SharedKernel.Exceptions;

namespace IdentityService.Application.Commands.User;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting user {UserId}", request.UserId);

        var user = await _userRepository.GetByIdAsync(request.UserId.ToString(), cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }

        // Soft delete the user
        await _userRepository.DeleteAsync(request.UserId.ToString(), cancellationToken);

        _logger.LogInformation("Successfully deleted user {UserId}", request.UserId);

        return Unit.Value;
    }
}
