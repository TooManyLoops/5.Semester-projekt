using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Timegrip.Shifts.Api.Data;
using Timegrip.Shifts.Api.Enums;
using Timegrip.Shifts.Api.Models;
using Timegrip.Shifts.Api.Requests;
using Timegrip.Shifts.Api.Services;

namespace Shifts.Api.Tests;

public sealed class ShiftAssignmentIntegrationTests : IAsyncLifetime
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
    public async Task AssignShiftToEmployeeRole_WhenShiftExists_ShouldPersistAssignment()
    {
        await using var context = CreateContext();
        await context.Database.MigrateAsync();

        var shift = new Shift
        {
            ShiftId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(9),
        };
        context.Shifts.Add(shift);
        await context.SaveChangesAsync();

        var service = new ShiftService(context);
        var request = new ShiftAssignmentRequest
        {
            ShiftId = shift.ShiftId,
            EmployeeRoleId = Guid.NewGuid(),
            AssignmentStatus = AssignmentStatus.Confirmed,
        };

        var assigned = await service.AssignShiftToEmployeeRole(request);

        assigned.Should().BeTrue();
        context.ShiftAssignments.Should().ContainSingle(assignment =>
            assignment.ShiftId == shift.ShiftId &&
            assignment.EmployeeRoleId == request.EmployeeRoleId);
    }

    [Fact]
    public async Task AssignShiftToEmployeeRole_WhenShiftDoesNotExist_ShouldThrowNotFound()
    {
        await using var context = CreateContext();
        await context.Database.MigrateAsync();

        var service = new ShiftService(context);
        var request = new ShiftAssignmentRequest
        {
            ShiftId = Guid.NewGuid(),
            EmployeeRoleId = Guid.NewGuid(),
            AssignmentStatus = AssignmentStatus.Confirmed,
        };

        var act = () => service.AssignShiftToEmployeeRole(request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    private ShiftsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ShiftsDbContext>()
            .UseSqlServer(_database.GetConnectionString())
            .Options;

        return new ShiftsDbContext(options);
    }
}
