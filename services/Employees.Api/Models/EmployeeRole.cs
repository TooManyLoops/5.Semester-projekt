using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timegrip.Employees.Api.Models;

[Table("EmployeeRoles")]
public class EmployeeRole
{
    [Key]
    [Column("employeeRole_Id")]
    public Guid EmployeeRoleId { get; set; }

    [Column("employee_Id")]
    public Guid EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee Employee { get; set; }

    [Column("role_Id")]
    public Guid RoleId { get; set; }

    [ForeignKey("RoleId")]
    public Role Role { get; set; }

    [Column("isPrimary")]
    public bool IsPrimary { get; set; }
}
