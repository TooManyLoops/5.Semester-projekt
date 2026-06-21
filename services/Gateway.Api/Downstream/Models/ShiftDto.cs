namespace Timegrip.Gateway.Api.Downstream.Models;

// Shape returned by Shifts.Api for a shift with optional requirements and assignments.
public sealed class ShiftDto
{
    public Guid ShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid? RoleId { get; set; }
    public bool IsAssigned { get; set; }
    public Guid? EmployeeRoleId { get; set; }
    public List<ShiftRequirementDto>? ShiftRequirements { get; set; }
    public List<ShiftAssignmentDto>? ShiftAssignments { get; set; }
}
