using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models;

public class EmployeeRoleRequest
{
    [Required]
    public Guid EmployeeId { get; set; }
    [Required]
    
    public Guid roleId { get; set; }
    [Required]
    
    public bool isPrimary { get; set; }
}