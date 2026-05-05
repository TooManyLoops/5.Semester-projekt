using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vagtplanlægnings_modul.Enums;

namespace Vagtplanlægnings_modul.Models;

public class Employment
{
    [Key]
    public Guid EmploymentId { get; set; }

    [Required]
    public Guid EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public Employee Employee { get; set; } = null!;

    [Required]
    public EmploymentType EmploymentType { get; set; }

    [Required]
    public int WeeklyHours { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
