namespace CatalogService.Contracts.Requests.Search;

public class GlobalSearchRequest
{
    public string SearchTerm { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string[] Types { get; set; } = Array.Empty<string>(); // Hotel, Package, Destination
    public string? Location { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MaxPrice { get; set; }
}
