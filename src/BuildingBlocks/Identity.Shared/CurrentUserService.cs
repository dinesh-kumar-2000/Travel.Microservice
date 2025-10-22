using Microsoft.AspNetCore.Http;
using SharedKernel.Utilities;

namespace Identity.Shared;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? TenantId { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User.GetUserId();

    public string? Email => _httpContextAccessor.HttpContext?.User.GetEmail();

    public string? TenantId => _httpContextAccessor.HttpContext?.User.GetTenantId();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User.GetRoles() ?? Enumerable.Empty<string>();
}

