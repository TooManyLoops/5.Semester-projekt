using Vagtplanlægnings_modul.Data;
using Vagtplanlægnings_modul.Models;
using Vagtplanlægnings_modul.Requests;
using Vagtplanlægnings_modul.Responses;

namespace Vagtplanlægnings_modul.Services;

public class EmployeeService(TimegripDbContext context)
{
    public async Task<EmployeeResponse> CreateEmployee(EmployeeRequest request)
    {
        var employee = new Employee()
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Status = request.EmployeeStatus,
            PhoneNumber = request.PhoneNumber,
        };

        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        return new EmployeeResponse
        {
            EmployeeId = employee.EmployeeId,
            Email = employee.Email,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            HiredAt = employee.HiredAt,
            EmployeeStatus = employee.Status,
            PhoneNumber = employee.PhoneNumber,
        };
    }
}
