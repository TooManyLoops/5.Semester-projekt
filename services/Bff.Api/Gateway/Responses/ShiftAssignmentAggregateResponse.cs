namespace Timegrip.Bff.Api.Gateway.Responses;

public sealed class ShiftAssignmentAggregateResponse
{
    public Guid ShiftAssignmentId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid EmployeeRoleId { get; set; }
    public byte Status { get; set; }
    public DateTime AssignedAt { get; set; }
}
