using MediatR;
using IdentityService.Contracts.Responses.User;
using SharedKernel.Models;

namespace IdentityService.Application.Queries.User;

public class GetAllUsersQuery : IRequest<PaginatedResult<UserResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}