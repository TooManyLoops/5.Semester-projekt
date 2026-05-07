using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models;

[Table("EmployeeRoles")]
public class EmployeeRole
{
    [Key]
    [Column("employeeRole_Id")]
    public Guid EmployeeRoleId { get; set; }

    [Column("employee_Id")]
    public Guid EmployeeId { get; set; }

    [ForeignKey("Employee_Id")]
    public Employee Employee { get; set; } = null!;

    [Column("role_Id")]
    public Guid RoleId { get; set; }

    [ForeignKey("Role_Id")]
    public Role Role { get; set; } = null!;

    [Column("isPrimary")]
    public bool isPrimary { get; set; }
}
