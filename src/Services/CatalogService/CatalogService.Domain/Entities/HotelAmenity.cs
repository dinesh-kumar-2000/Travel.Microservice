using SharedKernel.Models;

namespace CatalogService.Domain.Entities;

public class HotelAmenity : BaseEntity<string>
{
    public Guid HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsIncluded { get; set; } = true;
}
