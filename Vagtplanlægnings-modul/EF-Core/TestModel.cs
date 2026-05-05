using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Vagtplanlægnings_modul.EF_Core
{
    public class TestModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int Phone { get; set; }
        public TestModel(string name, int phone)
        {
            Name = name;
            Phone = phone;
        }
    }
}
