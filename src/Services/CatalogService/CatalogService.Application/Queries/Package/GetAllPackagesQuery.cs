using MediatR;
using CatalogService.Contracts.Responses.Package;
using SharedKernel.Models;

namespace CatalogService.Application.Queries.Package;

public class GetAllPackagesQuery : IRequest<PaginatedResult<PackageResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}

