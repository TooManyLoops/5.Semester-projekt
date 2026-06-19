using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Timegrip.Employees.Api.Data;
using Timegrip.Employees.Api.Enums;
using Timegrip.Employees.Api.Requests;
using Timegrip.Employees.Api.Services;

namespace Employees.Api.Tests;

public sealed class EmployeeRoleIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
    }

    [Fact]
    public async Task CreateEmployeeRole_WhenEmployeeAndRoleExist_ShouldPersistRelation()
    {
        // Arrange
        await using var context = CreateContext();
        await context.Database.MigrateAsync();

        var employeeService = new EmployeeService(context);
        var roleService = new RoleService(context);
        var employeeRoleService = new EmployeeRoleService(context);

        var employee = await employeeService.CreateEmployee(new EmployeeRequest
        {
            FirstName = "Anna",
            LastName = "Andersen",
            Email = "anna@example.com",
            PhoneNumber = "12345678",
            EmployeeStatus = EmployeeStatus.Active,
        });
        var role = await roleService.CreateRole(new RoleRequest
        {
            Name = "Bartender",
            Description = "Can cover bar shifts",
        });

        var request = new EmployeeRoleRequest
        {
            EmployeeId = employee.EmployeeId,
            RoleId = role.RoleId,
            IsPrimary = true,
        };

        // Act
        var employeeRole = await employeeRoleService.CreateEmployeeRole(request);

        // Assert
        employeeRole.EmployeeRoleId.Should().NotBeEmpty();
        context.EmployeeRoles.Should().ContainSingle(relation =>
            relation.EmployeeId == employee.EmployeeId &&
            relation.RoleId == role.RoleId &&
            relation.IsPrimary);
    }

    [Fact]
    public async Task CreateEmployeeRole_WhenEmployeeDoesNotExist_ShouldFailBecauseDatabaseRejectsForeignKey()
    {
        // Arrange
        await using var context = CreateContext();
        await context.Database.MigrateAsync();

        var roleService = new RoleService(context);
        var employeeRoleService = new EmployeeRoleService(context);
        var role = await roleService.CreateRole(new RoleRequest
        {
            Name = "Bartender",
            Description = "Can cover bar shifts",
        });

        var request = new EmployeeRoleRequest
        {
            EmployeeId = Guid.NewGuid(),
            RoleId = role.RoleId,
            IsPrimary = true,
        };

        // Act
        var act = () => employeeRoleService.CreateEmployeeRole(request);

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    private EmployeesDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EmployeesDbContext>()
            .UseSqlServer(_database.GetConnectionString())
            .Options;

        return new EmployeesDbContext(options);
    }
}
