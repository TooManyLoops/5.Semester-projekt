using System.ComponentModel.DataAnnotations;
using Timegrip.Employees.Api.Enums;
using Timegrip.Employees.Api.Requests;
using Timegrip.Employees.Api.Services;

namespace Timegrip.Employees.Api.Endpoints;

public static class EmployeeEndpoints
{
    public static WebApplication MapEmployeeEndpoints(this WebApplication app)
    {
        var employees = app.MapGroup("/employees");

        employees.MapPost("/", CreateEmployee).WithName("CreateEmployee");
        employees.MapGet("/statuses", GetEmployeeStatuses).WithName("GetEmployeeStatuses");
        employees.MapGet("/", GetAllEmployees).WithName("GetEmployees");
        employees.MapGet("/{employeeId:guid}", GetEmployee).WithName("GetEmployee");
        employees.MapPut("/{employeeId:guid}", UpdateEmployee).WithName("UpdateEmployee");
        
        return app;
    }

    private static async Task<IResult> CreateEmployee(EmployeeService service, EmployeeRequest request)
    {
        var validationResults = Validate(request);

        if (validationResults.Count > 0)
        {
            return TypedResults.BadRequest(validationResults);
        }

        try
        {
            var result = await service.CreateEmployee(request);
            return TypedResults.Created($"/employees/{result.EmployeeId}", result);
        }
        catch (InvalidOperationException exception)
        {
            return TypedResults.Conflict(exception.Message);
        }
    }

    private static async Task<IResult> GetEmployeeStatuses(EmployeeService service)
    {
        var result = await service.GetEmployeeStatuses();
        return TypedResults.Ok(result);
    }

    //Takes an optional pagination parameter, if not provided, it will return all employees.
    private static async Task<IResult> GetAllEmployees(EmployeeService service, PaginationRequest? pagination)
    {
        var result = await service.GetAllEmployees(pagination);
        return TypedResults.Ok(result);
    }

    
    
    private static async Task<IResult> GetEmployee(EmployeeService service, Guid employeeId)
    {
        var result = await service.GetEmployee(employeeId);

        return result is null
            ? TypedResults.NotFound(new { Message = "Employee was not found.", EmployeeId = employeeId })
            : TypedResults.Ok(result);
    }

    private static async Task<IResult> UpdateEmployee(
        EmployeeService service,
        Guid employeeId,
        UpdateEmployeeRequest request)
    {
        var result = await service.UpdateEmployee(employeeId, request);

        return result is null
            ? TypedResults.NotFound(new { Message = "Employee was not found.", EmployeeId = employeeId })
            : TypedResults.Ok(result);
    }
    
    

    private static List<ValidationResult> Validate<T>(T model)
    {
        var validationResults = new List<ValidationResult>();

        Validator.TryValidateObject(
            model!,
            new ValidationContext(model!),
            validationResults,
            validateAllProperties: true);

        return validationResults;
    }
}
