using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timegrip.Roles.Api.Models;

[Table("EmployeeRoles")]
public class EmployeeRole
{
    [Key]
    [Column("employeeRole_Id")]
    public Guid EmployeeRoleId { get; set; }

    [Column("employee_Id")]
    public Guid EmployeeId { get; set; }

    [Column("role_Id")]
    public Guid RoleId { get; set; }

    [Column("isPrimary")]
    public bool IsPrimary { get; set; }
}
