namespace Timegrip.Employees.Api.Responses;

public class RoleResponse
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}
