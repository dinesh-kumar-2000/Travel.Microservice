using MediatR;
using Microsoft.Extensions.Logging;
using IdentityService.Domain.Repositories;
using IdentityService.Contracts.DTOs;
using SharedKernel.Exceptions;
using Identity.Shared;

namespace IdentityService.Application.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing refresh token for user {UserId}", request.UserId);

        // Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            throw new NotFoundException("User not found");
        }

        // Validate refresh token
        if (string.IsNullOrEmpty(user.RefreshToken) || user.RefreshToken != request.RefreshToken)
        {
            _logger.LogWarning("Invalid refresh token for user {UserId}", request.UserId);
            throw new UnauthorizedException("Invalid refresh token");
        }

        // Check if refresh token is expired
        if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token expired for user {UserId}", request.UserId);
            throw new UnauthorizedException("Refresh token has expired");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("User {UserId} is deactivated", request.UserId);
            throw new ForbiddenException("Account is deactivated");
        }

        // Get user roles
        var roles = (await _userRepository.GetUserRolesAsync(user.Id, cancellationToken)).ToList();

        if (!roles.Any())
        {
            _logger.LogWarning("User {UserId} has no assigned roles", request.UserId);
            throw new ForbiddenException("User has no assigned roles");
        }

        // Generate new tokens
        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, user.TenantId, roles);
        var newRefreshToken = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Update user with new refresh token
        user.SetRefreshToken(newRefreshToken, expiresAt);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Successfully refreshed token for user {UserId}", request.UserId);

        return new RefreshTokenResponse(
            accessToken,
            newRefreshToken,
            expiresAt
        );
    }
}

