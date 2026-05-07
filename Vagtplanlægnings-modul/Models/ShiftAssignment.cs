using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models
{
    public class ShiftAssignment
    {
        [Key]
        [Column("ShiftAssignment_Id")]
        public Guid ShiftAssignmentId { get; set; }

        [Column("shift_Id")]
        public Guid ShiftId { get; set; }

        [ForeignKey("shift_Id")]
        public Shift Shift { get; set; } = null!;

        [Column("employeeRole_Id")]
        public Guid EmployeeRoleId { get; set; }

        [Column("Status")]
        public byte Status { get; set; }

        [Column("Assigned_At")]
        public DateTime AssignedAt { get; set; }
    }
}
