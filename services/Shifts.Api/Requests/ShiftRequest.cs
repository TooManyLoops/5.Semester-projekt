namespace Timegrip.Shifts.Api.Requests;

public class ShiftRequest
{
    public Guid? ShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<ShiftRequirementRequest>? ShiftRequirements { get; set; }
    
}
