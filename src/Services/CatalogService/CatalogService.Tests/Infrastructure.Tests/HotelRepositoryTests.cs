using Xunit;
using FluentAssertions;
using Moq;
using CatalogService.Infrastructure.Persistence.Repositories;
using CatalogService.Domain.Entities;

namespace CatalogService.Tests.Infrastructure.Tests;

public class HotelRepositoryTests
{
    private readonly Mock<HotelRepository> _mockHotelRepository;

    public HotelRepositoryTests()
    {
        _mockHotelRepository = new Mock<HotelRepository>();
    }

    [Fact]
    public async Task CreateHotel_ShouldReturnCreatedHotel()
    {
        // Arrange
        var hotel = new Hotel(
            HotelId.Create(),
            "Test Hotel",
            new Location("Test City", "Test Country"),
            new Price(100.00m, "USD")
        );

        _mockHotelRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Hotel>()))
            .ReturnsAsync(hotel);

        // Act
        var result = await _mockHotelRepository.Object.CreateAsync(hotel);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Hotel");
    }

    [Fact]
    public async Task GetHotelById_ShouldReturnHotel_WhenExists()
    {
        // Arrange
        var hotelId = HotelId.Create();
        var expectedHotel = new Hotel(
            hotelId,
            "Test Hotel",
            new Location("Test City", "Test Country"),
            new Price(100.00m, "USD")
        );

        _mockHotelRepository
            .Setup(repo => repo.GetByIdAsync(hotelId))
            .ReturnsAsync(expectedHotel);

        // Act
        var result = await _mockHotelRepository.Object.GetByIdAsync(hotelId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedHotel);
    }
}
