using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models
{
    public class ShiftRequirement
    {
        //[Key]
        //[Column("ShiftRequirement_Id")]
        public Guid ShiftRequirementId { get; set; } = Guid.NewGuid();

        //[Required]
        //[Column("FK_Shift_Id")]
        //[ForeignKey("Shift")]
        public Guid ShiftId { get; set; }
        public Shift Shift { get; set; }

        //[Required]
        //[Column("FK_Role_Id")]
        public Guid RoleId { get; set; }

        //[Required]
        //[Column("Amount")]
        public byte Amount { get; set; }

    }
}
