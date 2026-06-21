namespace Timegrip.Gateway.Api.Downstream.Requests;

public sealed class ShiftRequirementDownstreamRequest
{
    public Guid RoleId { get; set; }
    public byte Amount { get; set; }
}
