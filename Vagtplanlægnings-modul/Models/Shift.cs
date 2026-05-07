using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vagtplanlægnings_modul.Models
{
    [Table("Shifts")]
    public class Shift
    {
        [Key]
        [Column("Shift_Id")]
        public Guid ShiftId { get; set; } = Guid.NewGuid();

       
        [Column("StartTime")]
        public DateTime StartTime { get; set; }

        
        [Column("EndTime")]
        public DateTime EndTime { get; set; }
    }
}
