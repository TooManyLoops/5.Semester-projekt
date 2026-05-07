using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timegrip.Shifts.Api.Models;

public class ShiftRequirement
{
    [Key]
    [Column("ShiftRequirement_Id")]
    public Guid ShiftRequirementId { get; set; } = Guid.NewGuid();

    public Guid ShiftId { get; set; }

    [ForeignKey(nameof(ShiftId))]
    public Shift Shift { get; set; } = null!;

    [Column("Role_Id")]
    public Guid RoleId { get; set; }

    [Column("Amount")]
    public byte Amount { get; set; }
}
