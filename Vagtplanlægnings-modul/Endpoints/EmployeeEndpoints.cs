using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;
using Vagtplanlægnings_modul.Requests;
using Vagtplanlægnings_modul.Services;

namespace Vagtplanlægnings_modul.Endpoints;

public static class EmployeeEndpoints
{
    public static WebApplication MapEmployeeEndpoint(this WebApplication app)
    {
        var employeeEndpoint = app.MapGroup("/Employees");
        employeeEndpoint.MapPost("/", CreateEmployee).WithName("CreateEmployee");
        employeeEndpoint.MapGet("/statuses", GetEmployeeStatuses).WithName("GetEmployeeStatuses");
        employeeEndpoint.MapGet("/all", GetAllEmployees).WithName("GetEmployees");
        employeeEndpoint.MapGet("/{employeeId}", GetEmployee).WithName("GetEmployee");

        return app;
    }

    public static async Task<IResult> GetEmployee(EmployeeService service, Guid employeeId)
    {
        var result = await service.GetEmployee(employeeId);

        if (result is null)
        {
            return TypedResults.NotFound(
                new { Message = "Employee was not found.", EmployeeId = employeeId }
            );
        }
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> GetAllEmployees(EmployeeService service)
    {
        var result = await service.GetAllEmployees();
        return TypedResults.Ok(result);
    }

    public static async Task<IResult> CreateEmployee(
        EmployeeService service,
        EmployeeRequest request
    )
    {
        var validationResults = Validate(request);

        if (validationResults.Count > 0)
        {
            return TypedResults.BadRequest(validationResults);
        }

        try
        {
            var result = await service.CreateEmployee(request);
            return TypedResults.Created($"/Employees/{result.EmployeeId}", result);
        }
        catch (InvalidOperationException exception)
        {
            return TypedResults.Conflict(exception.Message);
        }
    }

    public static async Task<IResult> GetEmployeeStatuses(EmployeeService service)
    {
        var result = await service.GetEmployeeStatuses();
        return TypedResults.Ok(result);
    }

    public static List<ValidationResult> Validate<T>(T model)
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
