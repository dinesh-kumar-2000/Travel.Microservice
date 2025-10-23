using MediatR;
using CatalogService.Application.DTOs.Responses.Package;

namespace CatalogService.Application.Queries.Package;

public class GetPackageByIdQuery : IRequest<PackageResponse?>
{
    public Guid PackageId { get; set; }
}

