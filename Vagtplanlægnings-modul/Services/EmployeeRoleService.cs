using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vagtplanlægnings_modul.Data;
using Vagtplanlægnings_modul.Requests;
using Vagtplanlægnings_modul.Responses;

namespace Vagtplanlægnings_modul.Models;

public class EmployeeRoleService(TimegripDbContext context)
{

    public async Task<EmployeeRoleResponse> CreateEmployeeRole(EmployeeRoleRequest request)
    {
        var employeeRole = new EmployeeRole()
        {
            Employee = request.Employee,
            role = request.role,
            isPrimary = request.isPrimary
        };

        context.EmployeeRoles.Add(employeeRole);
        await context.SaveChangesAsync();

        return new EmployeeRoleResponse
        {
            EmployeeRoleId = employeeRole.EmployeeRoleId,
            Employee = employeeRole.Employee,
            role = employeeRole.role,
            isPrimary = employeeRole.isPrimary
        };

    }
}