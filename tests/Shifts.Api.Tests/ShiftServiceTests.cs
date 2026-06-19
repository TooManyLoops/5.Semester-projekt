using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Timegrip.Shifts.Api.Data;
using Timegrip.Shifts.Api.Requests;
using Timegrip.Shifts.Api.Services;

namespace Shifts.Api.Tests;

public sealed class ShiftServiceTests
{
    [Fact]
    public async Task ValidateCreateShift_WhenTimesAreValid_ShouldCreateShift()
    {
        await using var context = CreateContext();
        var service = new ShiftService(context);
        var request = new ShiftRequest
        {
            StartTime = DateTime.Now.AddHours(1),
            EndTime = DateTime.Now.AddHours(9),
        };

        var shift = await service.ValidateCreateShift(request);

        shift.ShiftId.Should().NotBeEmpty();
        context.Shifts.Should().ContainSingle(storedShift => storedShift.ShiftId == shift.ShiftId);
    }

    [Fact]
    public async Task ValidateCreateShift_WhenEndTimeIsBeforeStartTime_ShouldThrowArgumentException()
    {
        await using var context = CreateContext();
        var service = new ShiftService(context);
        var request = new ShiftRequest
        {
            StartTime = DateTime.Now.AddHours(8),
            EndTime = DateTime.Now.AddHours(1),
        };

        var act = () => service.ValidateCreateShift(request);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("En eller flere datoer er ikke indtastet korrekt");
    }

    private static ShiftsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ShiftsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ShiftsDbContext(options);
    }
}
