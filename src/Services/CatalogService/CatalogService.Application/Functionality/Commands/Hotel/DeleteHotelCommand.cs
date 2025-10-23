using MediatR;
using Microsoft.Extensions.Logging;
using CatalogService.Application.Interfaces;
using Tenancy;

namespace CatalogService.Application.Commands.Hotel;

public record DeleteHotelCommand(string Id) : IRequest<bool>;

public class DeleteHotelCommandHandler : IRequestHandler<DeleteHotelCommand, bool>
{
    private readonly IHotelRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteHotelCommandHandler> _logger;

    public DeleteHotelCommandHandler(
        IHotelRepository repository,
        ITenantContext tenantContext,
        ILogger<DeleteHotelCommandHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteHotelCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting hotel {HotelId} for tenant {TenantId}", request.Id, _tenantContext.TenantId);

        var hotel = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (hotel == null)
            return false;

        await _repository.DeleteAsync(hotel.Id, cancellationToken);
        return true;
    }
}
