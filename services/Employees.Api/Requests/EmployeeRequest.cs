using System.ComponentModel.DataAnnotations;
using Timegrip.Employees.Api.Enums;

namespace Timegrip.Employees.Api.Requests;

public class EmployeeRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(50)]
    [MinLength(5)]
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    [MaxLength(50)]
    [MinLength(5)]
    public string PhoneNumber { get; set; } = null!;

    public EmployeeStatus EmployeeStatus { get; set; }
}
