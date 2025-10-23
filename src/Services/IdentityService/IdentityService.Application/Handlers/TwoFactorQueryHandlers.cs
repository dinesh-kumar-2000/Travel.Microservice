using IdentityService.Application.Queries;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Handlers;

public class GetTwoFactorStatusQueryHandler 
    : IRequestHandler<GetTwoFactorStatusQuery, TwoFactorStatusDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetTwoFactorStatusQueryHandler> _logger;

    public GetTwoFactorStatusQueryHandler(
        IUserRepository userRepository,
        ILogger<GetTwoFactorStatusQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<TwoFactorStatusDto> Handle(
        GetTwoFactorStatusQuery request,
        CancellationToken cancellationToken)
    {
        var twoFactorAuth = await _userRepository.GetTwoFactorAuthAsync(request.UserId);

        if (twoFactorAuth == null)
        {
            return new TwoFactorStatusDto(false, 0);
        }

        var backupCodesRemaining = await _userRepository.GetBackupCodesRemainingAsync(request.UserId);

        return new TwoFactorStatusDto(twoFactorAuth.Enabled, backupCodesRemaining);
    }
}

