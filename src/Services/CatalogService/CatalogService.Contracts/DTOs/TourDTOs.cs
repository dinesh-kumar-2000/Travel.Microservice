using System.Text.Json.Serialization;

namespace CatalogService.Contracts.DTOs;

public record CreateTourRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("destination")] string Destination,
    [property: JsonPropertyName("locations")] string[] Locations,
    [property: JsonPropertyName("durationDays")] int DurationDays,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("maxGroupSize")] int MaxGroupSize,
    [property: JsonPropertyName("startDate")] DateTime StartDate,
    [property: JsonPropertyName("endDate")] DateTime EndDate,
    [property: JsonPropertyName("inclusions")] string[]? Inclusions = null,
    [property: JsonPropertyName("exclusions")] string[]? Exclusions = null,
    [property: JsonPropertyName("images")] string[]? Images = null,
    [property: JsonPropertyName("difficulty")] string? Difficulty = null,
    [property: JsonPropertyName("languages")] string[]? Languages = null,
    [property: JsonPropertyName("minAge")] int MinAge = 0,
    [property: JsonPropertyName("meetingPoint")] string? MeetingPoint = null,
    [property: JsonPropertyName("guideInfo")] string? GuideInfo = null
);

public record UpdateTourRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("inclusions")] string[]? Inclusions = null,
    [property: JsonPropertyName("exclusions")] string[]? Exclusions = null,
    [property: JsonPropertyName("images")] string[]? Images = null
);

public record TourDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("destination")] string Destination,
    [property: JsonPropertyName("locations")] string[] Locations,
    [property: JsonPropertyName("durationDays")] int DurationDays,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("maxGroupSize")] int MaxGroupSize,
    [property: JsonPropertyName("availableSpots")] int AvailableSpots,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("startDate")] DateTime StartDate,
    [property: JsonPropertyName("endDate")] DateTime EndDate,
    [property: JsonPropertyName("inclusions")] string[] Inclusions,
    [property: JsonPropertyName("exclusions")] string[] Exclusions,
    [property: JsonPropertyName("images")] string[] Images,
    [property: JsonPropertyName("difficulty")] string Difficulty,
    [property: JsonPropertyName("languages")] string[] Languages,
    [property: JsonPropertyName("minAge")] int MinAge,
    [property: JsonPropertyName("meetingPoint")] string? MeetingPoint,
    [property: JsonPropertyName("guideInfo")] string? GuideInfo,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);

public record SearchToursRequest(
    [property: JsonPropertyName("destination")] string? Destination = null,
    [property: JsonPropertyName("minDuration")] int? MinDuration = null,
    [property: JsonPropertyName("maxDuration")] int? MaxDuration = null,
    [property: JsonPropertyName("minPrice")] decimal? MinPrice = null,
    [property: JsonPropertyName("maxPrice")] decimal? MaxPrice = null,
    [property: JsonPropertyName("difficulty")] string? Difficulty = null,
    [property: JsonPropertyName("startDate")] DateTime? StartDate = null,
    [property: JsonPropertyName("page")] int Page = 1,
    [property: JsonPropertyName("pageSize")] int PageSize = 10
);

public record PagedToursResponse(
    [property: JsonPropertyName("tours")] IEnumerable<TourDto> Tours,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("totalPages")] int TotalPages
);

