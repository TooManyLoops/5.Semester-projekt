namespace Timegrip.Bff.Api.Gateway.Responses;

public sealed class EmployeeRoleSummaryResponse : RoleSummaryResponse
{
    public Guid EmployeeRoleId { get; set; }
    public bool IsPrimary { get; set; }
}
