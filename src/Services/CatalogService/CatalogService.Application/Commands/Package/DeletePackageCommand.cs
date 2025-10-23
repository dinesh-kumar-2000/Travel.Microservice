using MediatR;

namespace CatalogService.Application.Commands.Package;

public class DeletePackageCommand : IRequest<bool>
{
    public Guid PackageId { get; set; }
}

