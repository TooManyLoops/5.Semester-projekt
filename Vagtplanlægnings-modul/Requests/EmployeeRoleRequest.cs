using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models;

public class EmployeeRoleRequest
{
    public Guid EmployeeRoleId  { get; set; }
    
    public Guid EmployeeId { get; set; }
    
    public Guid roleId { get; set; }
    
    public bool isPrimary { get; set; }
}