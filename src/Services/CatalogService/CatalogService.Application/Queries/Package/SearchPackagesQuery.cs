using MediatR;
using CatalogService.Contracts.Responses.Package;
using SharedKernel.Models;

namespace CatalogService.Application.Queries.Package;

public class SearchPackagesQuery : IRequest<PaginatedResult<PackageResponse>>
{
    public string? SearchTerm { get; set; }
    public string? PackageType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Duration { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

