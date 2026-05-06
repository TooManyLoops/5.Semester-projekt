using Microsoft.AspNetCore.Http.HttpResults;
using Vagtplanlægnings_modul.Requests;
using Vagtplanlægnings_modul.Services;

namespace Vagtplanlægnings_modul.Endpoints
{
    public static class ShiftEndpoints
    {
        public static WebApplication MapUserEndpoint(this WebApplication app)
        {
            var userEndpoint = app.MapGroup("/Shift");
            userEndpoint.MapPost("/", CreateShift).WithName("CreateShift");

            return app;
        }

        public static async Task<IResult> CreateShift(
            EmployeeService service,
            EmployeeRequest request
        )
        {
            var result = await service.CreateEmployee(request);
            return TypedResults.Created($"/Employees/{result.EmployeeId}", result);
        }

    }
}
