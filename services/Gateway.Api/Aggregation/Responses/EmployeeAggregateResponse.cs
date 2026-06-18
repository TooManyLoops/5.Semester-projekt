namespace Timegrip.Gateway.Api.Aggregation.Responses;

// Frontend read model for an employee enriched with assigned role details.
public sealed class EmployeeAggregateResponse
{
    public Guid EmployeeId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int EmployeeStatus { get; set; }
    public DateOnly HiredAt { get; set; }
    public List<EmployeeRoleSummaryResponse> Roles { get; set; } = [];
    public string RolesText { get; set; } = string.Empty;
}
