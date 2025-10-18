using MediatR;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Contracts.DTOs;
using IdentityService.Contracts.Events;
using SharedKernel.Exceptions;
using SharedKernel.Utilities;
using EventBus.Interfaces;

namespace IdentityService.Application.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventBus _eventBus;

    public RegisterUserCommandHandler(IUserRepository userRepository, IEventBus eventBus)
    {
        _userRepository = userRepository;
        _eventBus = eventBus;
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
            request.PhoneNumber
        );

        await _userRepository.AddAsync(user, cancellationToken);

        // Assign default Customer role
        var customerRoleId = "role_customer"; // This should come from seed data
        await _userRepository.AssignRoleAsync(userId, customerRoleId, cancellationToken);

        // Publish event
        await _eventBus.PublishAsync(new UserRegisteredEvent
        {
            UserId = userId,
            TenantId = request.TenantId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        }, cancellationToken);

        return new RegisterUserResponse(userId, request.Email, "User registered successfully");
    }
}

