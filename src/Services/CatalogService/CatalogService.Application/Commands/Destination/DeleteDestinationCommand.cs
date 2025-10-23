using MediatR;

namespace CatalogService.Application.Commands.Destination;

public class DeleteDestinationCommand : IRequest<Unit>
{
    public Guid DestinationId { get; set; }
}
