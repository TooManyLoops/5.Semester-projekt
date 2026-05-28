using Microsoft.EntityFrameworkCore;
using Timegrip.Employees.Api.Data;
using Timegrip.Employees.Api.Models;
using Timegrip.Employees.Api.Requests;
using Timegrip.Employees.Api.Responses;

namespace Timegrip.Employees.Api.Services;

public class EmployeeRoleService(EmployeesDbContext context)
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
        return await context
            .EmployeeRoles.AsNoTracking()
            .Where(employeeRole => employeeRole.EmployeeId == employeeId)
            .Select(employeeRole => ToResponse(employeeRole))
            .ToListAsync();
    }

    public async Task<EmployeeRoleResponse?> UpdateEmployeeRole(
        Guid employeeRoleId,
        UpdateEmployeeRoleRequest request
    )
    {
        var employeeRole = await context.EmployeeRoles.FindAsync(employeeRoleId);

        if (employeeRole is null)
        {
            return null;
        }

        if (request.IsPrimary is not null)
        {
            employeeRole.IsPrimary = request.IsPrimary.Value;
        }

        await context.SaveChangesAsync();
        return ToResponse(employeeRole);
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
