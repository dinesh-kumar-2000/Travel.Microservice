using MediatR;
using CatalogService.Contracts.Responses.Destination;

namespace CatalogService.Application.Commands.Destination;

public class UpdateDestinationCommand : IRequest<DestinationResponse>
{
    public Guid DestinationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int DestinationType { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public List<string> Images { get; set; } = new();
    public List<string> Attractions { get; set; } = new();
    public string? BestTimeToVisit { get; set; }
    public string? Climate { get; set; }
}
