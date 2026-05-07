using Microsoft.EntityFrameworkCore;
using Timegrip.Employees.Api.Data;
using Timegrip.Employees.Api.Enums;
using Timegrip.Employees.Api.Models;
using Timegrip.Employees.Api.Requests;
using Timegrip.Employees.Api.Responses;

namespace Timegrip.Employees.Api.Services;

public class EmployeeService(EmployeesDbContext context)
{
    public async Task<EmployeeResponse> CreateEmployee(EmployeeRequest request)
    {
        var email = request.Email.Trim();

        var emailAlreadyExists = await context.Employees.AnyAsync(employee =>
            employee.Email == email
        );

        if (emailAlreadyExists)
        {
            throw new InvalidOperationException("An employee with this email already exists.");
        }

        var employee = new Employee
        {
            EmployeeId = Guid.NewGuid(),
            Email = email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Status = request.EmployeeStatus,
            PhoneNumber = request.PhoneNumber,
        };

        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        return ToResponse(employee);
    }

    public Task<List<EmployeeStatusResponse>> GetEmployeeStatuses()
    {
        var statuses = Enum.GetValues<EmployeeStatus>()
            .Select(status => new EmployeeStatusResponse
            {
                Value = (int)status,
                Name = status.ToString(),
            })
            .ToList();

        return Task.FromResult(statuses);
    }

    public async Task<List<EmployeeResponse>> GetAllEmployees()
    {
        return await context
            .Employees.AsNoTracking()
            .Select(employee => ToResponse(employee))
            .ToListAsync();
    }

    public async Task<EmployeeResponse?> GetEmployee(Guid employeeId)
    {
        return await context
            .Employees.AsNoTracking()
            .Where(employee => employee.EmployeeId == employeeId)
            .Select(employee => ToResponse(employee))
            .FirstOrDefaultAsync();
    }

    public async Task<EmployeeResponse?> UpdateEmployee(
        Guid employeeId,
        UpdateEmployeeRequest request
    )
    {
        var employee = await context.Employees.FindAsync(employeeId);

        if (employee is null)
        {
            return null;
        }

        if (request.Email is not null)
        {
            employee.Email = request.Email.Trim();
        }

        if (request.FirstName is not null)
        {
            employee.FirstName = request.FirstName;
        }

        if (request.LastName is not null)
        {
            employee.LastName = request.LastName;
        }

        if (request.EmployeeStatus.HasValue)
        {
            employee.Status = request.EmployeeStatus.Value;
        }

        if (request.PhoneNumber is not null)
        {
            employee.PhoneNumber = request.PhoneNumber;
        }

        await context.SaveChangesAsync();
        return ToResponse(employee);
    }

    private static EmployeeResponse ToResponse(Employee employee)
    {
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
