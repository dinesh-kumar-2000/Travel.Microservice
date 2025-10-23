namespace CatalogService.Contracts.Requests.Package;

public class SearchPackagesRequest
{
    public string? SearchTerm { get; set; }
    public string? PackageType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Duration { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

