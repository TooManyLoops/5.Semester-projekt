using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Timegrip.Shifts.Api.Data;
using Timegrip.Shifts.Api.Enums;
using Timegrip.Shifts.Api.Requests;
using Timegrip.Shifts.Api.Services;

namespace Shifts.Api.Tests;

public sealed class ShiftServiceTests
{
    [Fact]
    public async Task ValidateCreateShift_WhenTimesAreValid_ShouldCreateShift()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context);
        var request = new ShiftRequest
        {
            StartTime = DateTime.Now.AddHours(1),
            EndTime = DateTime.Now.AddHours(9),
        };

        // Act
        var shift = await service.ValidateCreateShift(request);

        // Assert
        shift.ShiftId.Should().NotBeEmpty();
        context.Shifts.Should().ContainSingle(storedShift => storedShift.ShiftId == shift.ShiftId);
    }

    [Fact]
    public async Task ValidateCreateShift_WhenEndTimeIsBeforeStartTime_ShouldThrowArgumentException()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context);
        var request = new ShiftRequest
        {
            StartTime = DateTime.Now.AddHours(8),
            EndTime = DateTime.Now.AddHours(1),
        };

        // Act
        var act = () => service.ValidateCreateShift(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("En eller flere datoer er ikke indtastet korrekt");
    }

    [Fact]
    public async Task AssignShiftToEmployeeRole_WhenEmployeeRoleCannotBeVerified_ShouldThrow()
    {
        // Arrange
        await using var context = CreateContext();
        var service = CreateService(context, HttpStatusCode.NotFound);
        var request = new ShiftAssignmentRequest
        {
            ShiftId = Guid.NewGuid(),
            EmployeeRoleId = Guid.NewGuid(),
            AssignmentStatus = AssignmentStatus.Confirmed,
        };

        // Act
        var act = () => service.AssignShiftToEmployeeRole(request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Can't find employeeRole. Either wrong employeeRoleId or doesnt exist");
    }

    private static ShiftsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ShiftsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ShiftsDbContext(options);
    }

    private static ShiftService CreateService(
        ShiftsDbContext context,
        HttpStatusCode verificationStatusCode = HttpStatusCode.OK)
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
