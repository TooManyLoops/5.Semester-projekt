using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models;

[Table("EmployeeRoles")]
public class EmployeeRoleResponse
{
 
    public Guid EmployeeRoleId  { get; set; }
    
    public Employee Employee { get; set; }
    
    public Role role { get; set; }
    
    public bool isPrimary { get; set; }
}