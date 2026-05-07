using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models;

[Table("EmployeeRoles")]
public class EmployeeRole
{
    [Key]
    [Column("EmployeeRole_Id")]
    public Guid EmployeeRoleId  { get; set; }
        
    public Guid EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee Employee { get; set; }

    
    public Guid RoleId { get; set; }

    [ForeignKey("Role_Id")]
    public Role Role { get; set; }

    [Column("isPrimary")]
    public bool isPrimary { get; set; }
}