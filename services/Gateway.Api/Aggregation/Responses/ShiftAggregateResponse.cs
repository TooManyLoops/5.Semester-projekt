namespace Timegrip.Gateway.Api.Aggregation.Responses;

// Frontend read model for a shift enriched with requirement role names.
public sealed class ShiftAggregateResponse
{
    public Guid ShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<ShiftRequirementAggregateResponse> ShiftRequirements { get; set; } = [];
    public List<ShiftAssignmentAggregateResponse> ShiftAssignments { get; set; } = [];
}
