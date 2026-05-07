using System.ComponentModel.DataAnnotations;

namespace Vagtplanlægnings_modul.Requests;

public class RoleRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;
}
