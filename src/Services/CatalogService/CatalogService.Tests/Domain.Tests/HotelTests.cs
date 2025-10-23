using Xunit;
using FluentAssertions;
using CatalogService.Domain.Entities;
using CatalogService.Domain.ValueObjects;

namespace CatalogService.Tests.Domain.Tests;

public class HotelTests
{
    [Fact]
    public void Hotel_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var hotelId = HotelId.Create();
        var name = "Test Hotel";
        var location = new Location("Test City", "Test Country");
        var price = new Price(100.00m, "USD");

        // Act
        var hotel = new Hotel(hotelId, name, location, price);

        // Assert
        hotel.Should().NotBeNull();
        hotel.Id.Should().Be(hotelId);
        hotel.Name.Should().Be(name);
        hotel.Location.Should().Be(location);
        hotel.Price.Should().Be(price);
    }

    [Fact]
    public void Hotel_ShouldThrowException_WithInvalidName()
    {
        // Arrange
        var hotelId = HotelId.Create();
        var location = new Location("Test City", "Test Country");
        var price = new Price(100.00m, "USD");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Hotel(hotelId, "", location, price));
    }
}
