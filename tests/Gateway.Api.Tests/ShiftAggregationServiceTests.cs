using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Timegrip.Gateway.Api.Downstream;
using Timegrip.Gateway.Api.Aggregation.Requests;
using Timegrip.Gateway.Api.Aggregation.Services;

namespace Gateway.Api.Tests;

public sealed class ShiftAggregationServiceTests
{
    [Fact]
    public async Task GetShifts_WhenShiftHasRequirements_ShouldAddRoleNames()
    {
        var shiftId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var employeesHandler = new StubHttpMessageHandler()
            .MapJson("/roles/", new[]
            {
                new { RoleId = roleId, Name = "Chef", Description = "Kitchen" },
            });

        var shiftsHandler = new StubHttpMessageHandler()
            .MapJson("/shifts/", new[]
            {
                new
                {
                    ShiftId = shiftId,
                    StartTime = DateTime.UtcNow.AddHours(1),
                    EndTime = DateTime.UtcNow.AddHours(9),
                    ShiftRequirements = new[]
                    {
                        new
                        {
                            ShiftRequirementId = Guid.NewGuid(),
                            ShiftId = shiftId,
                            RoleId = roleId,
                            Amount = 1,
                        },
                    },
                    ShiftAssignments = Array.Empty<object>(),
                },
            });

        var service = CreateService(employeesHandler, shiftsHandler);

        var result = await service.GetShifts(CancellationToken.None);

        result.Should().ContainSingle();
        result[0].ShiftRequirements.Should().ContainSingle(requirement =>
            requirement.RoleId == roleId && requirement.RoleName == "Chef"
        );
    }

    [Fact]
    public async Task AssignShift_WhenEmployeeRoleDoesNotExist_ShouldReturnNotFoundResult()
    {
        var employeeRoleId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var shiftsHandler = new StubHttpMessageHandler();

        var service = CreateService(
            new StubHttpMessageHandler()
                .MapStatus($"/employee-roles/employeeRole/{employeeRoleId}", HttpStatusCode.NotFound),
            shiftsHandler
        );

        var result = await service.AssignShift(
            shiftId,
            new AssignShiftGatewayRequest
            {
                EmployeeRoleId = employeeRoleId,
                AssignmentStatus = 1,
            },
            CancellationToken.None
        );

        result.Should().Be(ShiftAssignmentResult.EmployeeRoleNotFound);
        shiftsHandler.Requests.Should().BeEmpty();
    }

    private static ShiftAggregationService CreateService(
        HttpMessageHandler employeesHandler,
        HttpMessageHandler shiftsHandler
    )
    {
        var employeesHttpClient = new HttpClient(employeesHandler)
        {
            BaseAddress = new Uri("http://employees.test"),
        };
        var shiftsHttpClient = new HttpClient(shiftsHandler)
        {
            BaseAddress = new Uri("http://shifts.test"),
        };

        var employeesApi = new EmployeesApiClient(employeesHttpClient);

        return new ShiftAggregationService(
            new ShiftsApiClient(shiftsHttpClient),
            employeesApi,
            new RoleCatalogService(employeesApi, new MemoryCache(new MemoryCacheOptions()))
        );
    }
}
