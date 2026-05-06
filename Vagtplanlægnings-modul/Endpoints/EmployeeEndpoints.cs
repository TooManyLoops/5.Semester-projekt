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
    public static WebApplication MapUserEndpoint(this WebApplication app)
    {
        var userEndpoint = app.MapGroup("/Employees");
        userEndpoint.MapPost("/", CreateEmployee).WithName("CreateEmployee");
        return app;
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

        var result = await service.CreateEmployee(request);
        return TypedResults.Created($"/Employees/{result.EmployeeId}", result);
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
