using Timegrip.Gateway.Api.Downstream;
using Timegrip.Gateway.Api.Downstream.Models;
using Timegrip.Gateway.Api.Aggregation.Responses;

namespace Timegrip.Gateway.Api.Aggregation.Services;

public sealed class EmployeeAggregationService(
    EmployeesApiClient employeesApi,
    RoleCatalogService roleCatalog
)
{
    // Builds the employee list view model by combining employees, cached role catalog, and live assignments.
    public async Task<List<EmployeeAggregateResponse>> GetEmployees(
        CancellationToken cancellationToken
    )
    {
        var employeesTask = employeesApi.GetEmployees(cancellationToken);
        var rolesTask = roleCatalog.GetRoles(cancellationToken);

        await Task.WhenAll(employeesTask, rolesTask);

        var employees = await employeesTask;
        var rolesById = (await rolesTask).ToDictionary(role => role.RoleId);

        // Current implementation asks for assignments per employee. A future bulk endpoint can remove this N+1 pattern.
        var employeeRoles = await Task.WhenAll(
            employees.Select(employee => employeesApi.GetEmployeeRoles(employee.EmployeeId, cancellationToken))
        );

        return employees
            .Select((employee, index) => ToAggregate(employee, employeeRoles[index], rolesById))
            .ToList();
    }

    // Builds the edit/profile view model with both assigned roles and selectable role catalog.
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

    // Supplies all reference data needed by the create-shift page in one frontend request.
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
        // Role assignments are live data; role names/descriptions come from the cached role catalog.
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
