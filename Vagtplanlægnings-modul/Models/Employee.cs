using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vagtplanlægnings_modul.Enums;

namespace Vagtplanlægnings_modul.Models;

public class Employee
{
    [Key]
    [Column("employee_Id")]
    public Guid EmployeeId { get; set; }

    [Column("firstName")]
    public string FirstName { get; set; } = null!;

    [Column("lastName")]
    public string LastName { get; set; } = null!;

    [Column("email")]
    public string Email { get; set; } = null!;

    [Column("phoneNr")]
    public string PhoneNumber { get; set; } = null!;

    [Column("employeeStatus")]
    public EmployeeStatus Status { get; set; }

    [DataType(DataType.Date)]
    [Column("hiredAt")]
    public DateOnly HiredAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
