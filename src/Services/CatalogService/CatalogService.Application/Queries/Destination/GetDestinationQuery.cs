using MediatR;
using CatalogService.Contracts.Responses.Destination;
using SharedKernel.Models;

namespace CatalogService.Application.Queries.Destination;

public class GetDestinationQuery : IRequest<DestinationResponse?>
{
    public Guid DestinationId { get; set; }
}
