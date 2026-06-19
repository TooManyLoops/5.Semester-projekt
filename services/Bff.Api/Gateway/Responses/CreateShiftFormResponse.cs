namespace Timegrip.Bff.Api.Gateway.Responses;

public sealed class CreateShiftFormResponse
{
    public List<EmployeeAggregateResponse> Employees { get; set; } = [];
    public List<RoleSummaryResponse> Roles { get; set; } = [];
}
