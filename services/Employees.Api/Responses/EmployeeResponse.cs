using Timegrip.Employees.Api.Enums;

namespace Timegrip.Employees.Api.Responses;

public class EmployeeResponse
{
    public Guid EmployeeId { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public EmployeeStatus EmployeeStatus { get; set; }
    public DateOnly HiredAt { get; set; }
}
