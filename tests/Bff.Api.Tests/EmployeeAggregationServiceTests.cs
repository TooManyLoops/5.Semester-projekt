using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Timegrip.Bff.Api.Downstream;
using Timegrip.Bff.Api.Gateway.Services;

namespace Bff.Api.Tests;

public sealed class EmployeeAggregationServiceTests
{
    [Fact]
    public async Task GetEmployees_WhenEmployeesHaveRoles_ShouldReturnEmployeesWithRolesText()
    {
        var employeeId = Guid.NewGuid();
        var primaryRoleId = Guid.NewGuid();
        var secondaryRoleId = Guid.NewGuid();

        var service = CreateService(
            new StubHttpMessageHandler()
                .MapJson("/employees/", new[]
                {
                    new
                    {
                        EmployeeId = employeeId,
                        Email = "anna@example.test",
                        FirstName = "Anna",
                        LastName = "Nielsen",
                        PhoneNumber = "12345678",
                        EmployeeStatus = 1,
                        HiredAt = DateOnly.FromDateTime(DateTime.UtcNow),
                    },
                })
                .MapJson("/roles/", new[]
                {
                    new { RoleId = primaryRoleId, Name = "Manager", Description = "Leads shifts" },
                    new { RoleId = secondaryRoleId, Name = "Cashier", Description = "Handles sales" },
                })
                .MapJson($"/employee-roles/employee/{employeeId}", new[]
                {
                    new
                    {
                        EmployeeRoleId = Guid.NewGuid(),
                        EmployeeId = employeeId,
                        RoleId = secondaryRoleId,
                        IsPrimary = false,
                    },
                    new
                    {
                        EmployeeRoleId = Guid.NewGuid(),
                        EmployeeId = employeeId,
                        RoleId = primaryRoleId,
                        IsPrimary = true,
                    },
                })
        );

        var result = await service.GetEmployees(CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Roles.Should().HaveCount(2);
        result[0].RolesText.Should().Be("Manager, Cashier");
    }

    [Fact]
    public async Task GetEmployees_WhenEmployeeHasNoRoles_ShouldReturnEmptyRolesText()
    {
        var employeeId = Guid.NewGuid();

        var service = CreateService(
            new StubHttpMessageHandler()
                .MapJson("/employees/", new[]
                {
                    new
                    {
                        EmployeeId = employeeId,
                        Email = "bo@example.test",
                        FirstName = "Bo",
                        LastName = "Hansen",
                        PhoneNumber = "87654321",
                        EmployeeStatus = 1,
                        HiredAt = DateOnly.FromDateTime(DateTime.UtcNow),
                    },
                })
                .MapJson("/roles/", Array.Empty<object>())
                .MapJson($"/employee-roles/employee/{employeeId}", Array.Empty<object>())
        );

        var result = await service.GetEmployees(CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Roles.Should().BeEmpty();
        result[0].RolesText.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCreateShiftForm_ShouldReturnEmployeesAndRoles()
    {
        var employeeId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var service = CreateService(
            new StubHttpMessageHandler()
                .MapJson("/employees/", new[]
                {
                    new
                    {
                        EmployeeId = employeeId,
                        Email = "chris@example.test",
                        FirstName = "Chris",
                        LastName = "Larsen",
                        PhoneNumber = "11223344",
                        EmployeeStatus = 1,
                        HiredAt = DateOnly.FromDateTime(DateTime.UtcNow),
                    },
                })
                .MapJson("/roles/", new[]
                {
                    new { RoleId = roleId, Name = "Barista", Description = "Makes coffee" },
                })
                .MapJson($"/employee-roles/employee/{employeeId}", Array.Empty<object>())
        );

        var result = await service.GetCreateShiftForm(CancellationToken.None);

        result.Employees.Should().ContainSingle();
        result.Roles.Should().ContainSingle(role => role.RoleId == roleId);
    }

    [Fact]
    public async Task GetEmployees_WhenDownstreamIsUnavailable_ShouldThrowUnavailableException()
    {
        var service = CreateService(new ThrowingHttpMessageHandler());

        var act = () => service.GetEmployees(CancellationToken.None);

        await act.Should().ThrowAsync<DownstreamUnavailableException>();
    }

    [Fact]
    public async Task GetEmployees_WhenCalledTwice_ShouldReuseCachedRoleCatalog()
    {
        var employeeId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var handler = new StubHttpMessageHandler()
            .MapJson("/employees/", new[]
            {
                new
                {
                    EmployeeId = employeeId,
                    Email = "dina@example.test",
                    FirstName = "Dina",
                    LastName = "Madsen",
                    PhoneNumber = "55667788",
                    EmployeeStatus = 1,
                    HiredAt = DateOnly.FromDateTime(DateTime.UtcNow),
                },
            })
            .MapJson("/roles/", new[]
            {
                new { RoleId = roleId, Name = "Host", Description = "Welcomes guests" },
            })
            .MapJson($"/employee-roles/employee/{employeeId}", new[]
            {
                new
                {
                    EmployeeRoleId = Guid.NewGuid(),
                    EmployeeId = employeeId,
                    RoleId = roleId,
                    IsPrimary = true,
                },
            });
        var service = CreateService(handler);

        await service.GetEmployees(CancellationToken.None);
        await service.GetEmployees(CancellationToken.None);

        handler.Requests
            .Count(request => request.RequestUri?.AbsolutePath == "/roles/")
            .Should()
            .Be(1);
    }

    private static EmployeeAggregationService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://employees.test"),
        };
        var employeesApi = new EmployeesApiClient(httpClient);

        return new EmployeeAggregationService(
            employeesApi,
            new RoleCatalogService(employeesApi, new MemoryCache(new MemoryCacheOptions()))
        );
    }
}
