namespace Timegrip.Roles.Api.Responses;

public class EmployeeRoleResponse
{
    public Guid EmployeeRoleId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid RoleId { get; set; }
    public bool IsPrimary { get; set; }
}
