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
    public Guid EmployeeId { get; set; }
    
    [ForeignKey("RoleId")]
    public Guid RoleId { get; set; }    
    
    [Column("isPrimary")]
    public bool isPrimary { get; set; }
}