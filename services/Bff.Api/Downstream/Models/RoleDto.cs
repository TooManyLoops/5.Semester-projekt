namespace Timegrip.Bff.Api.Downstream.Models;

public sealed class RoleDto
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
