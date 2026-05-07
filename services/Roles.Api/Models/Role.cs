using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timegrip.Roles.Api.Models;

[Table("Roles")]
public class Role
{
    [Key]
    [Column("role_Id")]
    public Guid RoleId { get; set; }

    [Column("role_name")]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string Description { get; set; } = null!;
}
