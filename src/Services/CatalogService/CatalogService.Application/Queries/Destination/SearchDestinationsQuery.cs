using MediatR;
using SharedKernel.Models;
using CatalogService.Contracts.Responses.Destination;

namespace CatalogService.Application.Queries.Destination;

public class SearchDestinationsQuery : IRequest<PaginatedResult<DestinationResponse>>
{
    public string? SearchTerm { get; set; }
    public int? DestinationType { get; set; }
    public string? Country { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
