using Timegrip.Shifts.Api.Enums;

namespace Timegrip.Shifts.Api.Requests;

public class ShiftAssignmentRequest
{
    [Required]
    public Guid ShiftId { get; set; }

    [Required]
    public Guid EmployeeId { get; set; }

    [Required]
    public AssignmentStatus AssignmentStatus { get; set; }
}
