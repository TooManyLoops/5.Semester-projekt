namespace Timegrip.Gateway.Api.Downstream.Models;

// Shape returned by Employees.Api for an employee-to-role assignment.
public sealed class EmployeeRoleDto
{
    public Guid EmployeeRoleId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid RoleId { get; set; }
    public bool IsPrimary { get; set; }
}
