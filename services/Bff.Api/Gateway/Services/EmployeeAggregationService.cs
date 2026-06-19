using Timegrip.Bff.Api.Downstream;
using Timegrip.Bff.Api.Downstream.Models;
using Timegrip.Bff.Api.Gateway.Responses;

namespace Timegrip.Bff.Api.Gateway.Services;

public sealed class EmployeeAggregationService(
    EmployeesApiClient employeesApi,
    RoleCatalogService roleCatalog
)
{
    public async Task<List<EmployeeAggregateResponse>> GetEmployees(
        CancellationToken cancellationToken
    )
    {
        var employeesTask = employeesApi.GetEmployees(cancellationToken);
        var rolesTask = roleCatalog.GetRoles(cancellationToken);

        await Task.WhenAll(employeesTask, rolesTask);

        var employees = await employeesTask;
        var rolesById = (await rolesTask).ToDictionary(role => role.RoleId);

        var employeeRoles = await Task.WhenAll(
            employees.Select(employee => employeesApi.GetEmployeeRoles(employee.EmployeeId, cancellationToken))
        );

        return employees
            .Select((employee, index) => ToAggregate(employee, employeeRoles[index], rolesById))
            .ToList();
    }

    public async Task<EmployeeDetailsAggregateResponse?> GetEmployeeDetails(
        Guid employeeId,
        CancellationToken cancellationToken
    )
    {
        var employeeTask = employeesApi.GetEmployee(employeeId, cancellationToken);
        var rolesTask = roleCatalog.GetRoles(cancellationToken);
        var employeeRolesTask = employeesApi.GetEmployeeRoles(employeeId, cancellationToken);

        await Task.WhenAll(employeeTask, rolesTask, employeeRolesTask);

        var employee = await employeeTask;
        if (employee is null)
        {
            return null;
        }

        var roles = await rolesTask;
        var rolesById = roles.ToDictionary(role => role.RoleId);

        return new EmployeeDetailsAggregateResponse
        {
            Employee = ToAggregate(employee, await employeeRolesTask, rolesById),
            AvailableRoles = roles.Select(ToRoleSummary).ToList(),
        };
    }

    public async Task<CreateShiftFormResponse> GetCreateShiftForm(
        CancellationToken cancellationToken
    )
    {
        var employeesTask = GetEmployees(cancellationToken);
        var rolesTask = roleCatalog.GetRoles(cancellationToken);

        await Task.WhenAll(employeesTask, rolesTask);

        return new CreateShiftFormResponse
        {
            Employees = await employeesTask,
            Roles = (await rolesTask).Select(ToRoleSummary).ToList(),
        };
    }

    private static EmployeeAggregateResponse ToAggregate(
        EmployeeDto employee,
        IReadOnlyCollection<EmployeeRoleDto> employeeRoles,
        IReadOnlyDictionary<Guid, RoleDto> rolesById
    )
    {
        var roles = employeeRoles
            .Select(employeeRole => ToEmployeeRoleSummary(employeeRole, rolesById))
            .OrderByDescending(role => role.IsPrimary)
            .ThenBy(role => role.Name)
            .ToList();

        return new EmployeeAggregateResponse
        {
            EmployeeId = employee.EmployeeId,
            Email = employee.Email,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            PhoneNumber = employee.PhoneNumber,
            EmployeeStatus = employee.EmployeeStatus,
            HiredAt = employee.HiredAt,
            Roles = roles,
            RolesText = roles.Count == 0
                ? string.Empty
                : string.Join(", ", roles.Select(role => role.Name)),
        };
    }

    private static EmployeeRoleSummaryResponse ToEmployeeRoleSummary(
        EmployeeRoleDto employeeRole,
        IReadOnlyDictionary<Guid, RoleDto> rolesById
    )
    {
        rolesById.TryGetValue(employeeRole.RoleId, out var role);

        return new EmployeeRoleSummaryResponse
        {
            EmployeeRoleId = employeeRole.EmployeeRoleId,
            RoleId = employeeRole.RoleId,
            Name = role?.Name ?? "Unknown role",
            Description = role?.Description ?? string.Empty,
            IsPrimary = employeeRole.IsPrimary,
        };
    }

    private static RoleSummaryResponse ToRoleSummary(RoleDto role) =>
        new()
        {
            RoleId = role.RoleId,
            Name = role.Name,
            Description = role.Description,
        };
}
