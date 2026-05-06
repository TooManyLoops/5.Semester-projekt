using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models;

[Table("EmployeeRoles")]
public class EmployeeRole
{
    [Key]
    [Column("EmployeeRoleId")]
    public Guid EmployeeRoleId  { get; set; }
    
    
    [ForeignKey("EmployeeId")]
    public Employee Employee { get; set; }
    
    [ForeignKey("RoleId")]
    public Role role { get; set; }
    
    [Column("isPrimary")]
    public bool isPrimary { get; set; }
}