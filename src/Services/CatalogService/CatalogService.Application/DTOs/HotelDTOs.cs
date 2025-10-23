using System.Text.Json.Serialization;

namespace CatalogService.Application.DTOs;

public record CreateHotelRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("starRating")] int StarRating,
    [property: JsonPropertyName("pricePerNight")] decimal PricePerNight,
    [property: JsonPropertyName("totalRooms")] int TotalRooms,
    [property: JsonPropertyName("amenities")] string[]? Amenities = null,
    [property: JsonPropertyName("images")] string[]? Images = null,
    [property: JsonPropertyName("latitude")] double? Latitude = null,
    [property: JsonPropertyName("longitude")] double? Longitude = null,
    [property: JsonPropertyName("contactEmail")] string? ContactEmail = null,
    [property: JsonPropertyName("contactPhone")] string? ContactPhone = null
);

public record UpdateHotelRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("pricePerNight")] decimal PricePerNight,
    [property: JsonPropertyName("amenities")] string[]? Amenities = null,
    [property: JsonPropertyName("images")] string[]? Images = null
);

public record HotelDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("starRating")] int StarRating,
    [property: JsonPropertyName("pricePerNight")] decimal PricePerNight,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("totalRooms")] int TotalRooms,
    [property: JsonPropertyName("availableRooms")] int AvailableRooms,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("amenities")] string[] Amenities,
    [property: JsonPropertyName("images")] string[] Images,
    [property: JsonPropertyName("latitude")] double? Latitude,
    [property: JsonPropertyName("longitude")] double? Longitude,
    [property: JsonPropertyName("contactEmail")] string? ContactEmail,
    [property: JsonPropertyName("contactPhone")] string? ContactPhone,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);

public record SearchHotelsRequest(
    [property: JsonPropertyName("city")] string? City = null,
    [property: JsonPropertyName("country")] string? Country = null,
    [property: JsonPropertyName("minPrice")] decimal? MinPrice = null,
    [property: JsonPropertyName("maxPrice")] decimal? MaxPrice = null,
    [property: JsonPropertyName("starRating")] int? StarRating = null,
    [property: JsonPropertyName("amenities")] string[]? Amenities = null,
    [property: JsonPropertyName("page")] int Page = 1,
    [property: JsonPropertyName("pageSize")] int PageSize = 10
);

public record PagedHotelsResponse(
    [property: JsonPropertyName("hotels")] IEnumerable<HotelDto> Hotels,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("totalPages")] int TotalPages
);

