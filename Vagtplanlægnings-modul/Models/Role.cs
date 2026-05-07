using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models;

[Table("Roles")]
public class Role
{
    [Key]
    [Column("role_Id")]
    public Guid RoleId { get; set; }
    
    [Required]
    [Column("role_name")]
    public String Name { get; set; }
    
    [Required]
    [Column("description")]
    public String Description { get; set; }

}