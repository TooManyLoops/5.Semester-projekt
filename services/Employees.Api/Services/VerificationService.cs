using Microsoft.EntityFrameworkCore;
using Timegrip.Employees.Api.Data;
using Timegrip.Employees.Api.Models;
using Timegrip.Employees.Api.Requests;
using Timegrip.Employees.Api.Responses;

namespace Timegrip.Employees.Api.Services;

public class VerificationService(EmployeesDbContext context)
{
    public async Task<bool> VerifyEmployeeRoleById(Guid employeeRoleId)
    {
        return context.EmployeeRoles.Any(e => e.EmployeeRoleId == employeeRoleId);
    }
}