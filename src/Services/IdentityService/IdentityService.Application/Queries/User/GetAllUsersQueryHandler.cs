using MediatR;
using Microsoft.Extensions.Logging;
using IdentityService.Domain.Repositories;
using IdentityService.Contracts.Responses.User;
using SharedKernel.Models;
using SharedKernel.Exceptions;

namespace IdentityService.Application.Queries.User;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PaginatedResult<UserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(
        IUserRepository userRepository,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<PaginatedResult<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all users with pagination - Page: {PageNumber}, Size: {PageSize}", 
            request.PageNumber, request.PageSize);

        // TODO: Implement proper pagination and search
        // For now, return empty result as this needs proper implementation
        var users = new List<UserResponse>();
        
        _logger.LogInformation("Retrieved {Count} users", users.Count);

        return PaginatedResult<UserResponse>.Create(
            users,
            0, // Total count
            request.PageNumber,
            request.PageSize);
    }
}
