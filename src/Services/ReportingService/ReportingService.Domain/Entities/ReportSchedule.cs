using SharedKernel.Models;

namespace ReportingService.Domain.Entities;

public class ReportSchedule : BaseEntity<string>
{
    public Guid ReportId { get; set; }
    public string Schedule { get; set; } = string.Empty;
    public string? EmailRecipients { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? NextRun { get; set; }
    public bool IsActive { get; set; } = true;
}
