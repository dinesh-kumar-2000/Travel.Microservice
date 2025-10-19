using MediatR;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Contracts.DTOs;
using IdentityService.Contracts.Events;
using SharedKernel.Exceptions;
using SharedKernel.Utilities;
using EventBus.Interfaces;
using Identity.Shared;

namespace IdentityService.Application.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventBus _eventBus;
    private readonly IJwtService _jwtService;

    public RegisterUserCommandHandler(
        IUserRepository userRepository, 
        IEventBus eventBus,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _eventBus = eventBus;
        _jwtService = jwtService;
    }

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email, request.TenantId, cancellationToken))
        {
            throw new ValidationException("Email", "Email already registered");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var userId = UlidGenerator.Generate();
        var user = User.Create(
            userId,
            request.TenantId,
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName,
            request.PhoneNumber ?? string.Empty
        );

        await _userRepository.AddAsync(user, cancellationToken);

        // Assign role (default to Customer if not specified)
        var roleName = request.Role ?? "Customer";
        var roleId = roleName switch
        {
            "SuperAdmin" => "role_superadmin",
            "TenantAdmin" => "role_tenantadmin",
            "Agent" => "role_agent",
            "Customer" => "role_customer",
            _ => "role_customer"
        };
        
        await _userRepository.AssignRoleAsync(userId, roleId, cancellationToken);

        // Get user roles
        var roles = await _userRepository.GetUserRolesAsync(userId, cancellationToken);

        // Generate tokens for immediate login after registration
        var accessToken = _jwtService.GenerateToken(userId, user.Email, user.TenantId, roles);
        var refreshToken = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Update user with refresh token
        user.SetRefreshToken(refreshToken, expiresAt);
        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Publish event
        await _eventBus.PublishAsync(new UserRegisteredEvent
        {
            UserId = userId,
            TenantId = request.TenantId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        }, cancellationToken);

        return new RegisterUserResponse(
            accessToken,
            refreshToken,
            expiresAt,
            new UserDto(userId, user.Email, user.FirstName, user.LastName, user.PhoneNumber, roles)
        );
    }
}

