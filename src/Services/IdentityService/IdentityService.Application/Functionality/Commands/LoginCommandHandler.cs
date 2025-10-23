using MediatR;
using IdentityService.Application.Interfaces;
using IdentityService.Application.DTOs;
using IdentityService.Application.Services;
using SharedKernel.Exceptions;
using Identity.Shared;

namespace IdentityService.Application.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IDomainResolutionService _domainResolutionService;

    public LoginCommandHandler(
        IUserRepository userRepository, 
        IJwtService jwtService,
        IDomainResolutionService domainResolutionService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _domainResolutionService = domainResolutionService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Resolve tenant ID from domain or use provided tenantId
        string? tenantId = request.TenantId;
        
        if (!string.IsNullOrEmpty(request.Domain))
        {
            // Check if domain is main domain (for SuperAdmin)
            if (_domainResolutionService.IsMainDomain(request.Domain))
            {
                tenantId = "system"; // Special tenant ID for SuperAdmin
            }
            else if (string.IsNullOrEmpty(tenantId))
            {
                // Resolve tenant from subdomain for tenant users
                tenantId = await _domainResolutionService.ResolveTenantIdFromDomainAsync(
                    request.Domain, 
                    cancellationToken);
                
                if (string.IsNullOrEmpty(tenantId))
                {
                    throw new NotFoundException("Tenant not found for the specified domain");
                }
            }
        }

        if (string.IsNullOrEmpty(tenantId))
        {
            throw new ValidationException("Domain", "Either domain or tenantId must be provided");
        }

        // Step 2: Get user by email and tenant
        var user = await _userRepository.GetByEmailAsync(request.Email, tenantId, cancellationToken);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException("Account is deactivated");
        }

        // Step 3: Get user roles
        var roles = (await _userRepository.GetUserRolesAsync(user.Id, cancellationToken)).ToList();

        if (!roles.Any())
        {
            throw new ForbiddenException("User has no assigned roles");
        }

        // Step 4: Validate role-domain combination (if domain was provided)
        if (!string.IsNullOrEmpty(request.Domain))
        {
            var primaryRole = roles.First(); // Get primary role
            
            if (!_domainResolutionService.ValidateRoleForDomain(primaryRole, request.Domain))
            {
                throw new ForbiddenException(
                    $"Users with role '{primaryRole}' cannot login from this domain");
            }
        }

        // Step 5: Generate tokens
        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, user.TenantId, roles);
        var refreshToken = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Step 6: Update user
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

