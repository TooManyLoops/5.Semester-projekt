namespace Timegrip.Shifts.Api.Requests;

public class ShiftRequest
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool AllowPast { get; set; }
    public List<ShiftRequirementRequest>? ShiftRequirements { get; set; }
    
}
