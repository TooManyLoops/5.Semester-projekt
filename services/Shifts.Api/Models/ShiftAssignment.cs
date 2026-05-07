using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timegrip.Shifts.Api.Models;

public class ShiftAssignment
{
    [Key]
    [Column("ShiftAssignment_Id")]
    public Guid ShiftAssignmentId { get; set; }

    [Column("shift_Id")]
    public Guid ShiftId { get; set; }

    [ForeignKey(nameof(ShiftId))]
    public Shift Shift { get; set; } = null!;

    [Column("employeeRole_Id")]
    public Guid EmployeeRoleId { get; set; }

    [Column("Status")]
    public byte Status { get; set; }

    [Column("Assigned_At")]
    public DateTime AssignedAt { get; set; }
}
