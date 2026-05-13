using Microsoft.EntityFrameworkCore;
using Timegrip.Employees.Api.Data;
using Timegrip.Employees.Api.Models;
using Timegrip.Employees.Api.Requests;
using Timegrip.Employees.Api.Responses;

namespace Timegrip.Employees.Api.Services;

public class RoleService(EmployeesDbContext context)
{
    public async Task<RoleResponse> CreateRole(RoleRequest request)
    {
        var role = new Role
        {
            RoleId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
        };

        context.Roles.Add(role);
        await context.SaveChangesAsync();

        return ToResponse(role);
    }

    public async Task<List<RoleResponse>> GetRoles()
    {
        return await context.Roles.AsNoTracking()
            .Select(role => ToResponse(role))
            .ToListAsync();
    }

    public async Task<RoleResponse?> GetRole(Guid roleId)
    {
        return await context.Roles.AsNoTracking()
            .Where(role => role.RoleId == roleId)
            .Select(role => ToResponse(role))
            .FirstOrDefaultAsync();
    }

    private static RoleResponse ToResponse(Role role)
    {
        return new RoleResponse
        {
            RoleId = role.RoleId,
            Name = role.Name,
            Description = role.Description,
        };
    }
}
