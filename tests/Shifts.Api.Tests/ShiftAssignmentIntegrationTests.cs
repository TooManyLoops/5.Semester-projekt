using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
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
    public async Task AssignShiftToEmployeeRole_WhenShiftExistsAndEmployeeRoleIsVerified_ShouldPersistAssignment()
    {
        // Arrange
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

        var service = CreateService(context, HttpStatusCode.OK);
        var request = new ShiftAssignmentRequest
        {
            ShiftId = shift.ShiftId,
            EmployeeRoleId = Guid.NewGuid(),
            AssignmentStatus = AssignmentStatus.Confirmed,
        };

        // Act
        var assigned = await service.AssignShiftToEmployeeRole(request);

        // Assert
        assigned.Should().BeTrue();
        context.ShiftAssignments.Should().ContainSingle(assignment =>
            assignment.ShiftId == shift.ShiftId &&
            assignment.EmployeeRoleId == request.EmployeeRoleId);
    }

    [Fact]
    public async Task AssignShiftToEmployeeRole_WhenShiftDoesNotExist_ShouldFailBecauseDatabaseRejectsForeignKey()
    {
        // Arrange
        await using var context = CreateContext();
        await context.Database.MigrateAsync();

        var service = CreateService(context, HttpStatusCode.OK);
        var request = new ShiftAssignmentRequest
        {
            ShiftId = Guid.NewGuid(),
            EmployeeRoleId = Guid.NewGuid(),
            AssignmentStatus = AssignmentStatus.Confirmed,
        };

        // Act
        var act = () => service.AssignShiftToEmployeeRole(request);

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    private ShiftsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ShiftsDbContext>()
            .UseSqlServer(_database.GetConnectionString())
            .Options;

        return new ShiftsDbContext(options);
    }

    private static ShiftService CreateService(ShiftsDbContext context, HttpStatusCode verificationStatusCode)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(verificationStatusCode));
        var retryPolicy = Policy.Handle<HttpRequestException>().RetryAsync(0);
        var verificationService = new VerificationService(
            httpClient,
            retryPolicy,
            NullLogger<VerificationService>.Instance);

        return new ShiftService(context, verificationService);
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent("{}"),
            });
        }
    }
}
