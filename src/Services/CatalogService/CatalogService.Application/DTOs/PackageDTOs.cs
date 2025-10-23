namespace CatalogService.Application.DTOs;

public record CreatePackageRequest(
    string Name,
    string Description,
    string Destination,
    int DurationDays,
    decimal Price,
    int MaxCapacity,
    DateTime StartDate,
    DateTime EndDate
);

public record PackageDto(
    string Id,
    string Name,
    string Description,
    string Destination,
    int DurationDays,
    decimal Price,
    string Currency,
    int AvailableSlots,
    string Status
);

public record SearchPackagesRequest(
    string? Destination,
    decimal? MinPrice,
    decimal? MaxPrice,
    int PageNumber = 1,
    int PageSize = 10
);

