namespace Timegrip.Gateway.Api.Downstream.Models;

// Shape returned by Shifts.Api for a shift assignment.
public sealed class ShiftAssignmentDto
{
    public Guid ShiftAssignmentId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid EmployeeRoleId { get; set; }
    public byte Status { get; set; }
    public DateTime AssignedAt { get; set; }
}
