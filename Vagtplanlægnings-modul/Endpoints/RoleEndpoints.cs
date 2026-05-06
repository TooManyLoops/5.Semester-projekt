using System.ComponentModel.DataAnnotations;
using Vagtplanlægnings_modul.Requests;
using Vagtplanlægnings_modul.Services;

namespace Vagtplanlægnings_modul.Endpoints;

public static class RoleEndpoints
{
    public static WebApplication MapRoleEndpoint(this WebApplication app)
    {
        var roleEndpoint = app.MapGroup("/Roles");
        roleEndpoint.MapPost("/", CreateRole).WithName("CreateRole");
        return app;
    }
    public static async Task<IResult> CreateRole(
        RoleService service,
        RoleRequest request
    )
    {
        var validationResults = Validate(request);

        if (validationResults.Count > 0)
        {
            return TypedResults.BadRequest(validationResults);
        }

        var result = await service.CreateRole(request);
        return TypedResults.Created($"/Roles/{result.RoleId}", result);
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