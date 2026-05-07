using System.ComponentModel.DataAnnotations;
using Vagtplanlægnings_modul.Models;
using Vagtplanlægnings_modul.Requests;
using Vagtplanlægnings_modul.Services;

namespace Vagtplanlægnings_modul.Endpoints;

public static class EmployeeRoleEndpoints
{
    public static WebApplication MapEmployeeRoleEndpoint(this WebApplication app)
    {
        var userEmployeeRoleEndpoint = app.MapGroup("/EmployeeRoles");
        userEmployeeRoleEndpoint.MapPost("/", CreateEmployeeRole).WithName("CreateEmployeeRole");
        return app;
    }
    public static async Task<IResult> CreateEmployeeRole(
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
        return TypedResults.Created($"/EmployeeRoles/{result.EmployeeRoleId}", result);
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