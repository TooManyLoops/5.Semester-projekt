using Microsoft.EntityFrameworkCore;
using Timegrip.Shifts.Api.Data;
using Timegrip.Shifts.Api.Models;
using Timegrip.Shifts.Api.Requests;
using Timegrip.Shifts.Api.Responses;

namespace Timegrip.Shifts.Api.Services;

public class ShiftService(ShiftsDbContext context, VerificationService verificationService)
{
    
    public async Task<ShiftResponse> ValidateCreateShift(ShiftRequest request)
    {
        if (!(request.StartTime >= DateTime.Now) || !(request.EndTime > request.StartTime))
        {
            throw new ArgumentException("En eller flere datoer er ikke indtastet korrekt");
        }
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

    private async Task<ShiftResponse> CreateShift(ShiftRequest request)
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

    private async Task<ShiftResponse> CreateShiftWithRequirements(ShiftRequest request)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
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
                    //Potentially make check for RoleId to be an empty GUID,
                    //as its not checked for in endpoint validation
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
        });
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

    public async Task<List<ShiftResponse>> GetAllShiftsForEmployeeId(Guid employeeRoleId)
    {
        var shiftAssignments = await context.ShiftAssignments
            .AsNoTracking()
            .Where(sa => sa.EmployeeRoleId == employeeRoleId)    
            .ToListAsync();

        var shiftList = new List<ShiftResponse>();
        foreach (ShiftAssignment sa in shiftAssignments)
        {
            var shift = await context.Shifts.AsNoTracking()
                .Where(s => s.ShiftId == sa.ShiftId)
                .FirstOrDefaultAsync();
            shiftList.Add(ToResponse(shift));
        }
        return shiftList;
    }

    public async Task<bool> AssignShiftToEmployeeRole(ShiftAssignmentRequest assignmentRequest)
    {
        var validationResult = await verificationService.VerifyEmployeeRoleById(assignmentRequest.EmployeeRoleId);
        if (validationResult is false)
        {
            throw new Exception("Can't find employeeRole. Either wrong employeeRoleId or doesnt exist");
        }
        
        var Shift = GetShift(assignmentRequest.ShiftId);
        if (Shift == null)
        {
            throw new KeyNotFoundException($"Shift with ID {assignmentRequest.ShiftId} not found");
        }

        var shiftAssignment = new ShiftAssignment
        {
            ShiftAssignmentId = Guid.NewGuid(),
            ShiftId = assignmentRequest.ShiftId,
            EmployeeRoleId = assignmentRequest.EmployeeRoleId,
            Status = (byte) assignmentRequest.AssignmentStatus,
            AssignedAt = DateTime.UtcNow
        };
        context.ShiftAssignments.Add(shiftAssignment);
        var result = await context.SaveChangesAsync();
        return result > 0;
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
