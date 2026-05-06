using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models
{
    public class ShiftAssignment
    {

        [Key]
        [Column("ShiftAssignment_Id")]
        public Guid ShiftAssignmentId { get; set; } = Guid.NewGuid();

        [Required]
        [Column("FK_Shift_Id")]
        [ForeignKey("Shift")]
        public Guid ShiftId { get; set; }
        public Shift Shift { get; set; }

        [Required]
        [Column("FK_EmployeeRole_Id")]
        public Guid EmployeeRoleId { get; set; }

        [Required]
        [Column("Status")]
        public byte Status { get; set; }

        [Required]
        [Column("Assigned_At")]
        public DateTime AssignedAt { get; set; }
    }
}
