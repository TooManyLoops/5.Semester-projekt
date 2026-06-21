namespace Timegrip.Gateway.Api.Downstream.Models;

// Shape returned by Employees.Api for the role catalog.
public sealed class RoleDto
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
