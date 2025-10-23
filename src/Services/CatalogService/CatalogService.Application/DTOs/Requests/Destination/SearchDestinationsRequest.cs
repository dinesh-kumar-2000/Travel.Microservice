using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.DTOs.Requests.Destination;

public class SearchDestinationsRequest
{
    public string? SearchTerm { get; set; }
    public int? DestinationType { get; set; }
    public string? Country { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
