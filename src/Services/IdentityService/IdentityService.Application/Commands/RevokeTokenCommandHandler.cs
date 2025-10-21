using MediatR;
using Microsoft.Extensions.Logging;
using IdentityService.Domain.Repositories;
using IdentityService.Contracts.DTOs;
using SharedKernel.Exceptions;

namespace IdentityService.Application.Commands;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, RevokeTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RevokeTokenCommandHandler> _logger;

    public RevokeTokenCommandHandler(
        IUserRepository userRepository,
        ILogger<RevokeTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<RevokeTokenResponse> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Revoking token for user {UserId}", request.UserId);

        // Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            throw new NotFoundException("User not found");
        }

        // Clear refresh token
        user.SetRefreshToken(string.Empty, DateTime.UtcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Successfully revoked token for user {UserId}", request.UserId);

        return new RevokeTokenResponse(
            true,
            "Token revoked successfully"
        );
    }
}

