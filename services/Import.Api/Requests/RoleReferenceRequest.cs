namespace Timegrip.Import.Api.Requests;

public sealed class RoleReferenceRequest
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
