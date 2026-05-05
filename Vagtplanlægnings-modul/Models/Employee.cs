using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vagtplanlægnings_modul.Enums;

namespace Vagtplanlægnings_modul.Models;

public class Employee
{
    [Key]
    [Column("employee_Id")]
    public Guid EmployeeId { get; set; }

    [Required]
    [Column("firstName")]
    public string FirstName { get; set; } = null!;

    [Required]
    [Column("lastName")]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    [Column("phoneNr")]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [Column("employeeStatus")]
    public EmployeeStatus Status { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Column("hiredAt")]
    public DateOnly HiredAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
