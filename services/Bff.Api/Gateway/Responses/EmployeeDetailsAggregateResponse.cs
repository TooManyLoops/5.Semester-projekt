namespace Timegrip.Bff.Api.Gateway.Responses;

public sealed class EmployeeDetailsAggregateResponse
{
    public EmployeeAggregateResponse Employee { get; set; } = new();
    public List<RoleSummaryResponse> AvailableRoles { get; set; } = [];
}
