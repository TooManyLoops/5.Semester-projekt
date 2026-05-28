using System.ComponentModel.DataAnnotations;
using Timegrip.Employees.Api.Requests;
using Timegrip.Employees.Api.Services;

namespace Timegrip.Employees.Api.Endpoints;

public static class RoleEndpoints
{
    public static WebApplication MapRoleEndpoints(this WebApplication app)
    {
        var roles = app.MapGroup("/roles");

        roles.MapPost("/", CreateRole).WithName("CreateRole");
        roles.MapGet("/", GetRoles).WithName("GetRoles");
        roles.MapGet("/{roleId:guid}", GetRole).WithName("GetRole");

        return app;
    }

    private static async Task<IResult> CreateRole(RoleService service, RoleRequest request)
    {
        var validationResults = Validate(request);

        if (validationResults.Count > 0)
        {
            return TypedResults.BadRequest(validationResults);
        }

        var result = await service.CreateRole(request);
        return TypedResults.Created($"/roles/{result.RoleId}", result);
    }

    private static async Task<IResult> GetRoles(RoleService service)
    {
        var result = await service.GetRoles();
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetRole(RoleService service, Guid roleId)
    {
        var result = await service.GetRole(roleId);

        return result is null
            ? TypedResults.NotFound(new { Message = "Role was not found.", RoleId = roleId })
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
