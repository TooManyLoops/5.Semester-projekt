using System.ComponentModel.DataAnnotations;

namespace Timegrip.Roles.Api.Requests;

public class EmployeeRoleRequest
{
    [Required]
    public Guid EmployeeId { get; set; }

    [Required]
    public Guid RoleId { get; set; }

    public bool IsPrimary { get; set; }
}
