namespace Timegrip.Gateway.Api.Aggregation.Responses;

// Frontend requirement summary with role name resolved from the role catalog.
public sealed class ShiftRequirementAggregateResponse
{
    public Guid ShiftRequirementId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public byte Amount { get; set; }
}
