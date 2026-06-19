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

                foreach (var requirement in request.ShiftRequirements ?? [])
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
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<List<ShiftResponse>> GetShifts()
    {
        return await context.Shifts
            .AsNoTracking()
            .Select(shift => new ShiftResponse
            {
                ShiftId = shift.ShiftId,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,

                RoleId = context.ShiftRequirements
                    .Where(requirement => requirement.ShiftId == shift.ShiftId)
                    .Select(requirement => (Guid?)requirement.RoleId)
                    .FirstOrDefault(),

                EmployeeRoleId = context.ShiftAssignments
                    .Where(assignment => assignment.ShiftId == shift.ShiftId)
                    .Select(assignment => (Guid?)assignment.EmployeeRoleId)
                    .FirstOrDefault(),

                IsAssigned = context.ShiftAssignments
                    .Any(assignment => assignment.ShiftId == shift.ShiftId)
            })
            .ToListAsync();
    }

    public async Task<ShiftResponse?> GetShift(Guid shiftId)
    {
        return await context.Shifts
            .AsNoTracking()
            .Where(shift => shift.ShiftId == shiftId)
            .Select(shift => new ShiftResponse
            {
                ShiftId = shift.ShiftId,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,

                RoleId = context.ShiftRequirements
                    .Where(requirement => requirement.ShiftId == shift.ShiftId)
                    .Select(requirement => (Guid?)requirement.RoleId)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<ShiftResponse>> GetAllShiftsForEmployeeRoleIds(List<Guid> employeeRoleIds)
    {

        var shiftIds = await context.ShiftAssignments
            .Where(sa => employeeRoleIds.Contains(sa.EmployeeRoleId))
            .Select(sa => sa.ShiftId)
            .Distinct()
            .ToListAsync();

        var shifts = await context.Shifts
            .Where(s => shiftIds.Contains(s.ShiftId))
            .ToListAsync();

        var assignments = await context.ShiftAssignments
            .Where(sa => shiftIds.Contains(sa.ShiftId))
            .ToListAsync();

        var requirements = await context.ShiftRequirements
            .Where(sr => shiftIds.Contains(sr.ShiftId))
            .ToListAsync();

        return shifts.Select(s => ToResponse(
            s,
            assignments.Where(sa => sa.ShiftId == s.ShiftId).ToList(),
            requirements.Where(sr => sr.ShiftId == s.ShiftId).ToList()
        )).ToList();
    }

    public async Task<bool> AssignShiftToEmployeeRole(ShiftAssignmentRequest assignmentRequest)
    {
        var validationResult = await verificationService.VerifyEmployeeRoleById(assignmentRequest.EmployeeRoleId);
        if (validationResult is false)
        {
            throw new Exception("Can't find employeeRole. Either wrong employeeRoleId or doesnt exist");
        }

        var shift = await GetShift(assignmentRequest.ShiftId);
        if (shift is null)
        {
            throw new KeyNotFoundException($"Shift with ID {assignmentRequest.ShiftId} not found");
        }

        var shiftAssignment = new ShiftAssignment
        {
            ShiftAssignmentId = Guid.NewGuid(),
            ShiftId = assignmentRequest.ShiftId,
            EmployeeRoleId = assignmentRequest.EmployeeRoleId,
            Status = (byte)assignmentRequest.AssignmentStatus,
            AssignedAt = DateTime.UtcNow
        };
        context.ShiftAssignments.Add(shiftAssignment);
        var result = await context.SaveChangesAsync();
        return result > 0;
    }

    private static ShiftResponse ToResponse(Shift shift, List<ShiftAssignment>? sa = null, List<ShiftRequirement>? sr = null)
    {
        return new ShiftResponse
        {
            ShiftId = shift.ShiftId,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            ShiftAssignments = sa,
            ShiftRequirements = sr
        };
    }
}
