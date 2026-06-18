namespace Timegrip.Gateway.Api.Aggregation.Responses;

// Frontend-safe role catalog entry.
public class RoleSummaryResponse
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
