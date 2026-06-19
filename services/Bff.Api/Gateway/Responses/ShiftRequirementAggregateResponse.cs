namespace Timegrip.Bff.Api.Gateway.Responses;

public sealed class ShiftRequirementAggregateResponse
{
    public Guid ShiftRequirementId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public byte Amount { get; set; }
}
