namespace Timegrip.Bff.Api.Downstream.Models;

public sealed class EmployeeDto
{
    public Guid EmployeeId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int EmployeeStatus { get; set; }
    public DateOnly HiredAt { get; set; }
}
