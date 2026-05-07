using System.ComponentModel.DataAnnotations;
using Vagtplanlægnings_modul.Enums;

namespace Vagtplanlægnings_modul.Requests
{
    public class UpdateEmployeeRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public EmployeeStatus? EmployeeStatus { get; set; }
    }
}
