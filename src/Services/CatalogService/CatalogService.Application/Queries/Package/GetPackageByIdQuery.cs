using MediatR;
using CatalogService.Contracts.Responses.Package;

namespace CatalogService.Application.Queries.Package;

public class GetPackageByIdQuery : IRequest<PackageResponse?>
{
    public Guid PackageId { get; set; }
}

