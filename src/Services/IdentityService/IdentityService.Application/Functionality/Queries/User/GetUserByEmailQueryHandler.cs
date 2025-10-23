using MediatR;
using Microsoft.Extensions.Logging;
using IdentityService.Application.Interfaces;
using IdentityService.Application.DTOs.Responses.User;
using SharedKernel.Exceptions;

namespace IdentityService.Application.Queries.User;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserResponse?>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserByEmailQueryHandler> _logger;

    public GetUserByEmailQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserByEmailQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserResponse?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user by email: {Email}", request.Email);

        // TODO: Get tenant ID from context
        var user = await _userRepository.GetByEmailAsync(request.Email, "default", cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("User with email {Email} not found", request.Email);
            return null;
        }

        // Get user roles
        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);

        _logger.LogInformation("Successfully retrieved user by email {Email}", request.Email);

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
