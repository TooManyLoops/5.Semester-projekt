using Microsoft.EntityFrameworkCore;
using Timegrip.Shifts.Api.Data;
using Timegrip.Shifts.Api.Models;
using Timegrip.Shifts.Api.Requests;
using Timegrip.Shifts.Api.Responses;

namespace Timegrip.Shifts.Api.Services;

public class ShiftService(ShiftsDbContext context)
{

    public async Task<ShiftResponse> ValidateCreateShift(ShiftRequest request)
    {
        if (request.ShiftRequirements is not null && request.ShiftRequirements.Any())
        {
            if (request.ShiftRequirements.Any(r =>
                r.RoleId == Guid.Empty ||
                r.Amount == 0))
                throw new ArgumentException("Alle felter i Requirements skal være udfyldt");
            return await CreateShiftWithRequirements(request);
        }
        return await CreateShift(request);
    }

    public async Task<ShiftResponse> CreateShift(ShiftRequest request)
    {   
        var shift = new Shift
        {
            ShiftId = Guid.NewGuid(),
            StartTime = request.StartTime,
            EndTime = request.EndTime,
        };

        context.Shifts.Add(shift);
        await context.SaveChangesAsync();

        return ToResponse(shift);
    }

    public async Task<ShiftResponse> CreateShiftWithRequirements(ShiftRequest request)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var shift = new Shift
            {
                ShiftId = Guid.NewGuid(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
            };
            context.Shifts.Add(shift);

            foreach (var requirement in request.ShiftRequirements)
            {
                var shiftRequirement = new ShiftRequirement
                {
                    ShiftId = shift.ShiftId,                    
                    Amount = requirement.Amount,
                    RoleId = requirement.RoleId,
                };
                context.ShiftRequirements.Add(shiftRequirement);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ToResponse(shift);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }        
    }

    public async Task<List<ShiftResponse>> GetShifts()
    {
        return await context.Shifts.AsNoTracking()
            .Select(shift => ToResponse(shift))
            .ToListAsync();
    }

    public async Task<ShiftResponse?> GetShift(Guid shiftId)
    {
        return await context.Shifts.AsNoTracking()
            .Where(shift => shift.ShiftId == shiftId)
            .Select(shift => ToResponse(shift))
            .FirstOrDefaultAsync();
    }

    private static ShiftResponse ToResponse(Shift shift)
    {
        return new ShiftResponse
        {
            ShiftId = shift.ShiftId,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
        };
    }
}
