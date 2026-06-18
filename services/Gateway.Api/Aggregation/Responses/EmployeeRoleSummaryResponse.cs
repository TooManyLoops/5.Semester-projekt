namespace Timegrip.Gateway.Api.Aggregation.Responses;

// Frontend role summary plus assignment metadata for one employee.
public sealed class EmployeeRoleSummaryResponse : RoleSummaryResponse
{
    public Guid EmployeeRoleId { get; set; }
    public bool IsPrimary { get; set; }
}
