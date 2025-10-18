using MediatR;
using IdentityService.Domain.Repositories;
using IdentityService.Contracts.DTOs;
using SharedKernel.Exceptions;
using Identity.Shared;

namespace IdentityService.Application.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId, cancellationToken);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException("Account is deactivated");
        }

        // Get user roles
        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);

        // Generate tokens
        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, user.TenantId, roles);
        var refreshToken = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Update user
        user.SetRefreshToken(refreshToken, expiresAt);
        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new LoginResponse(
            accessToken,
            refreshToken,
            expiresAt,
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.PhoneNumber, roles)
        );
    }
}

