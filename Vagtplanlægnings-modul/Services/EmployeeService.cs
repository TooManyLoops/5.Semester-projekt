using Microsoft.EntityFrameworkCore;
using Vagtplanlægnings_modul.Data;
using Vagtplanlægnings_modul.Enums;
using Vagtplanlægnings_modul.Models;
using Vagtplanlægnings_modul.Requests;
using Vagtplanlægnings_modul.Responses;

namespace Vagtplanlægnings_modul.Services;

public class EmployeeService(TimegripDbContext context)
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

        var employee = new Employee()
        {
            Email = email,
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

    public async Task<List<EmployeeStatusResponse>> GetEmployeeStatuses()
    {
        return Enum.GetValues<EmployeeStatus>()
            .Select(status => new EmployeeStatusResponse
            {
                Value = (int)status,
                Name = status.ToString(),
            })
            .ToList();
    }

    public async Task<List<EmployeeResponse>> GetAllEmployees()
    {
        return await context
            .Employees.Select(employee => new EmployeeResponse
            {
                EmployeeId = employee.EmployeeId,
                Email = employee.Email,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                HiredAt = employee.HiredAt,
                EmployeeStatus = employee.Status,
                PhoneNumber = employee.PhoneNumber,
            })
            .ToListAsync();
    }

    public async Task<EmployeeResponse?> GetEmployee(Guid employeeId)
    {
        return await context
            .Employees.AsNoTracking()
            .Where(employee => employee.EmployeeId == employeeId)
            .Select(employee => new EmployeeResponse
            {
                EmployeeId = employee.EmployeeId,
                Email = employee.Email,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                HiredAt = employee.HiredAt,
                EmployeeStatus = employee.Status,
                PhoneNumber = employee.PhoneNumber,
            })
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
