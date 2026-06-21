namespace Timegrip.Gateway.Api.Aggregation.Responses;

// Frontend-safe shift assignment summary; employee-role details can be enriched later.
public sealed class ShiftAssignmentAggregateResponse
{
    public Guid ShiftAssignmentId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid EmployeeRoleId { get; set; }
    public byte Status { get; set; }
    public DateTime AssignedAt { get; set; }
}
