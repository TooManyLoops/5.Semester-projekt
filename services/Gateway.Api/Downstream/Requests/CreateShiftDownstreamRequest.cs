namespace Timegrip.Gateway.Api.Downstream.Requests;

// Gateway-owned shape sent to Shifts.Api when orchestration creates shifts.
public sealed class CreateShiftDownstreamRequest
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool AllowPast { get; set; }
    public List<ShiftRequirementDownstreamRequest> ShiftRequirements { get; set; } = [];
}
