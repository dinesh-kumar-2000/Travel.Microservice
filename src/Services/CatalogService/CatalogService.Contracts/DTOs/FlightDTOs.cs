namespace CatalogService.Contracts.DTOs;

public record FlightDto(
    Guid Id,
    Guid TenantId,
    string FlightNumber,
    string Airline,
    string AircraftType,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    string Duration,
    string Status,
    decimal EconomyPrice,
    decimal BusinessPrice,
    decimal FirstClassPrice,
    int EconomySeats,
    int BusinessSeats,
    int FirstClassSeats,
    int EconomyAvailable,
    int BusinessAvailable,
    int FirstClassAvailable,
    string? BaggageAllowance,
    bool Meals,
    bool Wifi,
    string? Layovers,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateFlightRequest(
    string FlightNumber,
    string Airline,
    string AircraftType,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    string Duration,
    decimal EconomyPrice,
    decimal BusinessPrice,
    decimal FirstClassPrice,
    int EconomySeats,
    int BusinessSeats,
    int FirstClassSeats,
    string? BaggageAllowance,
    bool Meals,
    bool Wifi,
    string? Layovers,
    string? Notes
);

public record UpdateFlightRequest(
    string FlightNumber,
    string Airline,
    string AircraftType,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    string Duration,
    decimal EconomyPrice,
    decimal BusinessPrice,
    decimal FirstClassPrice,
    int EconomySeats,
    int BusinessSeats,
    int FirstClassSeats,
    string? BaggageAllowance,
    bool Meals,
    bool Wifi,
    string? Layovers,
    string? Notes,
    string Status
);

public record PagedResult<T>(
    List<T> Data,
    int CurrentPage,
    int TotalPages,
    int TotalCount
);
