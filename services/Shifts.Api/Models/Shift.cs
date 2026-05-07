using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timegrip.Shifts.Api.Models;

[Table("Shifts")]
public class Shift
{
    [Key]
    [Column("Shift_Id")]
    public Guid ShiftId { get; set; }

    [Column("startTime")]
    public DateTime StartTime { get; set; }

    [Column("endTime")]
    public DateTime EndTime { get; set; }
}
