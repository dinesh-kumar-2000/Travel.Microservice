using Xunit;
using FluentAssertions;
using Moq;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;

namespace CatalogService.Tests.Application.Tests;

public class HotelServiceTests
{
    private readonly Mock<IHotelRepository> _mockHotelRepository;

    public HotelServiceTests()
    {
        _mockHotelRepository = new Mock<IHotelRepository>();
    }

    [Fact]
    public async Task GetHotelById_ShouldReturnHotel_WhenHotelExists()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var expectedHotel = new Hotel
        {
            Id = hotelId,
            Name = "Test Hotel",
            Location = "Test Location"
        };

        _mockHotelRepository
            .Setup(repo => repo.GetByIdAsync(hotelId))
            .ReturnsAsync(expectedHotel);

        // Act
        var result = await _mockHotelRepository.Object.GetByIdAsync(hotelId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedHotel);
    }

    [Fact]
    public async Task GetHotelById_ShouldReturnNull_WhenHotelDoesNotExist()
    {
        // Arrange
        var hotelId = Guid.NewGuid();

        _mockHotelRepository
            .Setup(repo => repo.GetByIdAsync(hotelId))
            .ReturnsAsync((Hotel?)null);

        // Act
        var result = await _mockHotelRepository.Object.GetByIdAsync(hotelId);

        // Assert
        result.Should().BeNull();
    }
}
