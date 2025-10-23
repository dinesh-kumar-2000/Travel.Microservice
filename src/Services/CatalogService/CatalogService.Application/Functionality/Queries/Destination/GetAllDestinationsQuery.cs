using MediatR;
using SharedKernel.Models;
using CatalogService.Application.DTOs.Responses.Destination;

namespace CatalogService.Application.Queries.Destination;

public class GetAllDestinationsQuery : IRequest<PaginatedResult<DestinationResponse>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
