using SharedKernel.Models;

namespace CatalogService.Domain.Entities;

public class PackageItinerary : BaseEntity<string>
{
    public Guid PackageId { get; set; }
    public int Day { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<string> Activities { get; set; } = new();
    public string? Meals { get; set; }
    public string? Accommodation { get; set; }
    public string? Transportation { get; set; }
}
