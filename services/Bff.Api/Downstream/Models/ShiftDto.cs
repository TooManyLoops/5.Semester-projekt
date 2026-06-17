namespace Timegrip.Bff.Api.Downstream.Models;

public sealed class ShiftDto
{
    public Guid ShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<ShiftRequirementDto>? ShiftRequirements { get; set; }
    public List<ShiftAssignmentDto>? ShiftAssignments { get; set; }
}
