using MediatR;
using Microsoft.Extensions.Logging;
using CatalogService.Contracts.Responses.Package;
using CatalogService.Domain.Repositories;

namespace CatalogService.Application.Queries.Package;

public class GetPackageQuery : IRequest<PackageResponse?>
{
    public Guid PackageId { get; set; }
}

public class GetPackageQueryHandler : IRequestHandler<GetPackageQuery, PackageResponse?>
{
    private readonly IPackageRepository _repository;
    private readonly ILogger<GetPackageQueryHandler> _logger;

    public GetPackageQueryHandler(
        IPackageRepository repository,
        ILogger<GetPackageQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PackageResponse?> Handle(GetPackageQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting package {PackageId}", request.PackageId);

        var package = await _repository.GetByIdAsync(request.PackageId.ToString(), cancellationToken);

        if (package == null)
            return null;

        return new PackageResponse
        {
            Id = package.Id,
            TenantId = package.TenantId,
            Name = package.Name,
            Description = package.Description,
            PackageType = "Standard", // Default value since Package entity doesn't have this
            DestinationId = package.Destination, // Using Destination as DestinationId
            HotelId = string.Empty, // Not available in Package entity
            FlightId = string.Empty, // Not available in Package entity
            Price = package.Price,
            Currency = package.Currency,
            Duration = package.DurationDays,
            StartDate = package.StartDate,
            EndDate = package.EndDate,
            MaxTravelers = package.MaxCapacity,
            IncludedServices = package.Inclusions,
            ExcludedServices = package.Exclusions,
            Itinerary = null, // Not available in Package entity
            Images = Array.Empty<string>(), // Not available in Package entity
            Status = package.Status.ToString(),
            CreatedAt = package.CreatedAt
        };
    }
}
