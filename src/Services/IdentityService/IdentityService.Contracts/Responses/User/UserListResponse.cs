using IdentityService.Contracts.Responses.User;

namespace IdentityService.Contracts.Responses.User;

public class UserListResponse
{
    public List<UserResponse> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
