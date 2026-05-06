using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models;

[Table("EmployeeRoles")]
public class EmployeeRoleResponse
{
 
    public Guid EmployeeRoleId  { get; set; }
    
    public Guid EmployeeId { get; set; }
    
    public Guid roleId { get; set; }
    
    public bool isPrimary { get; set; }
}