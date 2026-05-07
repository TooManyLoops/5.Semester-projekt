using System.ComponentModel.DataAnnotations;
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

        return app;
    }

    private static async Task<IResult> CreateShift(ShiftService service, ShiftRequest request)
    {
        var validationResults = Validate(request);

        if (validationResults.Count > 0)
        {
            return TypedResults.BadRequest(validationResults);
        }

        var result = await service.CreateShift(request);
        return TypedResults.Created($"/shifts/{result.ShiftId}", result);
    }

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
