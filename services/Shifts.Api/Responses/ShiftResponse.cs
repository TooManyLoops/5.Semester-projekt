using Timegrip.Shifts.Api.Models;

namespace Timegrip.Shifts.Api.Responses;

public class ShiftResponse
{
    public Guid ShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<ShiftRequirement>? ShiftRequirements { get; set; }
    public List<ShiftAssignment>? ShiftAssignments { get; set; }
}
