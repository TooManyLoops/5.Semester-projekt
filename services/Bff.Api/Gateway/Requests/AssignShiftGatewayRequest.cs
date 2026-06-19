namespace Timegrip.Bff.Api.Gateway.Requests;

public sealed class AssignShiftGatewayRequest
{
    public Guid EmployeeRoleId { get; set; }
    public int AssignmentStatus { get; set; }
}
