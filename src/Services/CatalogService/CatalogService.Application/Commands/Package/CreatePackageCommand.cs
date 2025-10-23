using MediatR;
using CatalogService.Contracts.Responses.Package;

namespace CatalogService.Application.Commands.Package;

public class CreatePackageCommand : IRequest<PackageResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PackageType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Duration { get; set; }
    public Guid? DestinationId { get; set; }
    public Guid? HotelId { get; set; }
    public Guid? FlightId { get; set; }
    public List<string>? Inclusions { get; set; }
    public List<string>? Exclusions { get; set; }
    public string? Itinerary { get; set; }
    public List<string>? Images { get; set; }
}

