using MediatR;
using CatalogService.Application.DTOs.Responses.Destination;
using SharedKernel.Models;

namespace CatalogService.Application.Queries.Destination;

public class GetDestinationQuery : IRequest<DestinationResponse?>
{
    public Guid DestinationId { get; set; }
}
