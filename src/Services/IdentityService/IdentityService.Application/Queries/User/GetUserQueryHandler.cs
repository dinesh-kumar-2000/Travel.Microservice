using MediatR;
using Microsoft.Extensions.Logging;
using IdentityService.Domain.Repositories;
using IdentityService.Contracts.Responses.User;
using SharedKernel.Exceptions;

namespace IdentityService.Application.Queries.User;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserResponse?>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserQueryHandler> _logger;

    public GetUserQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserResponse?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user by ID: {UserId}", request.UserId);

        var user = await _userRepository.GetByIdAsync(request.UserId.ToString(), cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId);
            return null;
        }

        // Get user roles
        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);

        _logger.LogInformation("Successfully retrieved user {UserId}", request.UserId);

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
