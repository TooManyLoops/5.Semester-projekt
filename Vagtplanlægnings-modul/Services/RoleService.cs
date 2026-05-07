using Vagtplanlægnings_modul.Data;
using Vagtplanlægnings_modul.Models;
using Vagtplanlægnings_modul.Requests;
using Vagtplanlægnings_modul.Responses;

namespace Vagtplanlægnings_modul.Services;

public class RoleService(TimegripDbContext context)
{
    public async Task<RoleResponse> CreateRole(RoleRequest request)
    {
        var role = new Role()
        {
            Name = request.Name,
            Description = request.Description,
        };

        context.Roles.Add(role);
        await context.SaveChangesAsync();

        return new RoleResponse
        {
            RoleId = role.RoleId,
            Name = role.Name,
            Description = role.Description,
        };
    }
}