using MediatR;
using TenantService.Contracts.DTOs;
using TenantService.Domain.Entities;
using TenantService.Domain.Repositories;
using TenantService.Contracts.Events;
using SharedKernel.Utilities;
using SharedKernel.Exceptions;
using EventBus.Interfaces;

namespace TenantService.Application.Commands;

public record CreateTenantCommand(
    string Name,
    string Subdomain,
    string ContactEmail,
    string ContactPhone
) : IRequest<TenantDto>;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly ITenantRepository _repository;
    private readonly IEventBus _eventBus;

    public CreateTenantCommandHandler(ITenantRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.SubdomainExistsAsync(request.Subdomain, cancellationToken))
            throw new ValidationException("Subdomain", "Subdomain already exists");

        var tenantId = UlidGenerator.Generate();
        var tenant = Tenant.Create(tenantId, request.Name, request.Subdomain, request.ContactEmail, request.ContactPhone);

        await _repository.AddAsync(tenant, cancellationToken);

        await _eventBus.PublishAsync(new TenantCreatedEvent
        {
            TenantId = tenantId,
            Name = request.Name,
            Subdomain = request.Subdomain
        }, cancellationToken);

        return new TenantDto(tenant.Id, tenant.Name, tenant.Subdomain, tenant.ContactEmail, 
            tenant.Status.ToString(), tenant.Tier.ToString());
    }
}

