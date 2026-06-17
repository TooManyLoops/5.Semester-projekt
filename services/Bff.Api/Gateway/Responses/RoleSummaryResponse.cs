namespace Timegrip.Bff.Api.Gateway.Responses;

public class RoleSummaryResponse
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
