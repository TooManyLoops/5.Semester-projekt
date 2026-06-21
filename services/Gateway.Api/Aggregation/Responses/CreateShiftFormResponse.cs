namespace Timegrip.Gateway.Api.Aggregation.Responses;

// Frontend read model with all data needed to render the create-shift form.
public sealed class CreateShiftFormResponse
{
    public List<EmployeeAggregateResponse> Employees { get; set; } = [];
    public List<RoleSummaryResponse> Roles { get; set; } = [];
}
