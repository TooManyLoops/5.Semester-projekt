namespace Timegrip.Gateway.Api.Aggregation.Responses;

// Frontend read model for editing one employee and choosing from all available roles.
public sealed class EmployeeDetailsAggregateResponse
{
    public EmployeeAggregateResponse Employee { get; set; } = new();
    public List<RoleSummaryResponse> AvailableRoles { get; set; } = [];
}
