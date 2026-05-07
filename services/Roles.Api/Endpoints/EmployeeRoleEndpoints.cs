using System.ComponentModel.DataAnnotations;
using Timegrip.Roles.Api.Requests;
using Timegrip.Roles.Api.Services;

namespace Timegrip.Roles.Api.Endpoints;

public static class EmployeeRoleEndpoints
{
    public static WebApplication MapEmployeeRoleEndpoints(this WebApplication app)
    {
        var employeeRoles = app.MapGroup("/employee-roles");

        employeeRoles.MapPost("/", CreateEmployeeRole).WithName("CreateEmployeeRole");
        employeeRoles
            .MapGet("/employee/{employeeId:guid}", GetEmployeeRoles)
            .WithName("GetEmployeeRoles");

        employeeRoles
            .MapPatch("/{employeeRoleId:guid}", UpdateEmployeeRole)
            .WithName("UpdateEmployeeRole");
        return app;
    }

    private static async Task<IResult> CreateEmployeeRole(
        EmployeeRoleService service,
        EmployeeRoleRequest request
    )
    {
        var validationResults = Validate(request);

        if (validationResults.Count > 0)
        {
            return TypedResults.BadRequest(validationResults);
        }

        var result = await service.CreateEmployeeRole(request);
        return TypedResults.Created($"/employee-roles/{result.EmployeeRoleId}", result);
    }

    private static async Task<IResult> GetEmployeeRoles(
        EmployeeRoleService service,
        Guid employeeId
    )
    {
        var result = await service.GetEmployeeRoles(employeeId);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> UpdateEmployeeRole(
        EmployeeRoleService service,
        Guid employeeRoleId,
        UpdateEmployeeRoleRequest request
    )
    {
        var result = await service.UpdateEmployeeRole(employeeRoleId, request);

        return result is null
            ? TypedResults.NotFound(
                new { Message = "Employee role was not found.", EmployeeRoleId = employeeRoleId }
            )
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
