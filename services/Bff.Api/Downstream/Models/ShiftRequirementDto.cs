namespace Timegrip.Bff.Api.Downstream.Models;

public sealed class ShiftRequirementDto
{
    public Guid ShiftRequirementId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid RoleId { get; set; }
    public byte Amount { get; set; }
}
