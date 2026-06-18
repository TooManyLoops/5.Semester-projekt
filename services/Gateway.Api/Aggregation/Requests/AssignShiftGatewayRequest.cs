namespace Timegrip.Gateway.Api.Aggregation.Requests;

// Frontend request for assigning a role-backed employee to a shift through the gateway.
public sealed class AssignShiftGatewayRequest
{
    public Guid EmployeeRoleId { get; set; }
    public int AssignmentStatus { get; set; }
}
