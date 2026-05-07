namespace Vagtplanlægnings_modul.Requests;

public class RoleRequest
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

}