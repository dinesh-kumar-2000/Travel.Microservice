using System.Text.Json.Serialization;

namespace CatalogService.Contracts.DTOs;

public record CreateFlightRequest(
    [property: JsonPropertyName("flightNumber")] string FlightNumber,
    [property: JsonPropertyName("airline")] string Airline,
    [property: JsonPropertyName("departureAirport")] string DepartureAirport,
    [property: JsonPropertyName("arrivalAirport")] string ArrivalAirport,
    [property: JsonPropertyName("departureCity")] string DepartureCity,
    [property: JsonPropertyName("arrivalCity")] string ArrivalCity,
    [property: JsonPropertyName("departureCountry")] string DepartureCountry,
    [property: JsonPropertyName("arrivalCountry")] string ArrivalCountry,
    [property: JsonPropertyName("departureTime")] DateTime DepartureTime,
    [property: JsonPropertyName("arrivalTime")] DateTime ArrivalTime,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("totalSeats")] int TotalSeats,
    [property: JsonPropertyName("flightClass")] string FlightClass,
    [property: JsonPropertyName("aircraftType")] string? AircraftType = null,
    [property: JsonPropertyName("baggageAllowanceKg")] int? BaggageAllowanceKg = null,
    [property: JsonPropertyName("hasMeal")] bool HasMeal = false,
    [property: JsonPropertyName("isRefundable")] bool IsRefundable = false
);

public record UpdateFlightRequest(
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("status")] string Status
);

public record FlightDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("flightNumber")] string FlightNumber,
    [property: JsonPropertyName("airline")] string Airline,
    [property: JsonPropertyName("departureAirport")] string DepartureAirport,
    [property: JsonPropertyName("arrivalAirport")] string ArrivalAirport,
    [property: JsonPropertyName("departureCity")] string DepartureCity,
    [property: JsonPropertyName("arrivalCity")] string ArrivalCity,
    [property: JsonPropertyName("departureCountry")] string DepartureCountry,
    [property: JsonPropertyName("arrivalCountry")] string ArrivalCountry,
    [property: JsonPropertyName("departureTime")] DateTime DepartureTime,
    [property: JsonPropertyName("arrivalTime")] DateTime ArrivalTime,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("totalSeats")] int TotalSeats,
    [property: JsonPropertyName("availableSeats")] int AvailableSeats,
    [property: JsonPropertyName("flightClass")] string FlightClass,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("aircraftType")] string? AircraftType,
    [property: JsonPropertyName("baggageAllowanceKg")] int? BaggageAllowanceKg,
    [property: JsonPropertyName("hasMeal")] bool HasMeal,
    [property: JsonPropertyName("isRefundable")] bool IsRefundable,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);

public record SearchFlightsRequest(
    [property: JsonPropertyName("departureCity")] string? DepartureCity = null,
    [property: JsonPropertyName("arrivalCity")] string? ArrivalCity = null,
    [property: JsonPropertyName("departureDate")] DateTime? DepartureDate = null,
    [property: JsonPropertyName("minPrice")] decimal? MinPrice = null,
    [property: JsonPropertyName("maxPrice")] decimal? MaxPrice = null,
    [property: JsonPropertyName("flightClass")] string? FlightClass = null,
    [property: JsonPropertyName("airline")] string? Airline = null,
    [property: JsonPropertyName("page")] int Page = 1,
    [property: JsonPropertyName("pageSize")] int PageSize = 10
);

public record PagedFlightsResponse(
    [property: JsonPropertyName("flights")] IEnumerable<FlightDto> Flights,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("totalPages")] int TotalPages
);

