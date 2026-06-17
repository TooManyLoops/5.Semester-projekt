namespace Timegrip.Bff.Api.Downstream.Models;

public sealed class EmployeeRoleDto
{
    public Guid EmployeeRoleId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid RoleId { get; set; }
    public bool IsPrimary { get; set; }
}
