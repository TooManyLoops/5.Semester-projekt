using System.ComponentModel.DataAnnotations;
using Timegrip.Shifts.Api.Models;
using Timegrip.Shifts.Api.Requests;
using Timegrip.Shifts.Api.Services;

namespace Timegrip.Shifts.Api.Endpoints;

public static class ShiftEndpoints
{
    public static WebApplication MapShiftEndpoints(this WebApplication app)
    {
        var shifts = app.MapGroup("/shifts");

        shifts.MapPost("/", CreateShift).WithName("CreateShift");
        shifts.MapGet("/", GetShifts).WithName("GetShifts");
        shifts.MapGet("/{shiftId:guid}", GetShift).WithName("GetShift");
        shifts.MapGet("/{employeeRoleId:guid}", GetAllShiftsForEmployeeId).WithName("GetAllShiftsForEmployeeId");

        return app;
    }

    private static async Task<IResult> CreateShift(ShiftService service, ShiftRequest request)
    {
        var validationResults = Validate(request);

        if (validationResults.Count > 0)
        {
            return TypedResults.BadRequest(validationResults);
        }

        try
        {
            var result = await service.ValidateCreateShift(request);
            return TypedResults.Created($"/shifts/{result.ShiftId}", result);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return TypedResults.Problem("Der skete en uventet fejl");
        }
    }

    //Returns a list of all shifts in the system.
    private static async Task<IResult> GetShifts(ShiftService service)
    {
        var result = await service.GetShifts();
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetShift(ShiftService service, Guid shiftId)
    {
        var result = await service.GetShift(shiftId);

        return result is null
            ? TypedResults.NotFound(new { Message = "Shift was not found.", ShiftId = shiftId })
            : TypedResults.Ok(result);
    }

    //Returns a list of all shifts that are assigned to a specific employee, identified by their employee role ID.
    private static async Task<IResult> GetAllShiftsForEmployeeId(ShiftService service, Guid employeeRoleId)
    {
        if (employeeRoleId == Guid.Empty || employeeRoleId == null)
        {
            return TypedResults.BadRequest("Invalid employee role ID");
        }

        try
        {
            var result = await service.GetAllShiftsForEmployeeId(employeeRoleId);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return TypedResults.Problem("Der skete en uventet fejl");
        }

    }

    public static async Task<IResult> AssignShiftToEmployee(ShiftService service, ShiftAssignmentRequest assignmentRequest)
    {
        var validationResults = Validate(assignmentRequest);

        if (validationResults.Count > 0)
        {
            return TypedResults.BadRequest(validationResults);
        }
        if (assignmentRequest.ShiftId == Guid.Empty || assignmentRequest.EmployeeId == Guid.Empty)
        {
            return TypedResults.BadRequest("Invalid shift or employee ID");
        }

        try
        {
            var result = await service.AssignShiftToEmployee(assignmentRequest);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return TypedResults.Problem("Der skete en uventet fejl");
        }

        //Check if shift exists for extra security
        //Check if BLL layer makes sense to have a method for this, or if it should be done in the controller.
    }

    private static List<ValidationResult> Validate<T>(T model)
    {
        var validationResults = new List<ValidationResult>();

        Validator.TryValidateObject(
            model!,
            new ValidationContext(model!),
            validationResults,
            validateAllProperties: true
        );

        return validationResults;
    }
}
