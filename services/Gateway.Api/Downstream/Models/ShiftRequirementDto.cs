namespace Timegrip.Gateway.Api.Downstream.Models;

// Shape returned by Shifts.Api for role requirements on a shift.
public sealed class ShiftRequirementDto
{
    public Guid ShiftRequirementId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid RoleId { get; set; }
    public byte Amount { get; set; }
}
