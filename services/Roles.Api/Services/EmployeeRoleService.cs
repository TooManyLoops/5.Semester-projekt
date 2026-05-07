using Microsoft.EntityFrameworkCore;
using Timegrip.Roles.Api.Data;
using Timegrip.Roles.Api.Models;
using Timegrip.Roles.Api.Requests;
using Timegrip.Roles.Api.Responses;

namespace Timegrip.Roles.Api.Services;

public class EmployeeRoleService(RolesDbContext context)
{
    public async Task<EmployeeRoleResponse> CreateEmployeeRole(EmployeeRoleRequest request)
    {
        var employeeRole = new EmployeeRole
        {
            EmployeeRoleId = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            RoleId = request.RoleId,
            IsPrimary = request.IsPrimary,
        };

        context.EmployeeRoles.Add(employeeRole);
        await context.SaveChangesAsync();

        return ToResponse(employeeRole);
    }

    public async Task<List<EmployeeRoleResponse>> GetEmployeeRoles(Guid employeeId)
    {
        return await context.EmployeeRoles.AsNoTracking()
            .Where(employeeRole => employeeRole.EmployeeId == employeeId)
            .Select(employeeRole => ToResponse(employeeRole))
            .ToListAsync();
    }

    private static EmployeeRoleResponse ToResponse(EmployeeRole employeeRole)
    {
        return new EmployeeRoleResponse
        {
            EmployeeRoleId = employeeRole.EmployeeRoleId,
            EmployeeId = employeeRole.EmployeeId,
            RoleId = employeeRole.RoleId,
            IsPrimary = employeeRole.IsPrimary,
        };
    }
}
